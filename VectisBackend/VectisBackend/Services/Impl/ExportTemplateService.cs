using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VectisBackend.Data;
using VectisBackend.Entities.Fk;
using VectisBackend.Services;

namespace VectisBackend.Services.Impl;

public class ExportTemplateService : IExportTemplateService
{
    private readonly VectisDbContext _db;

    // Nombre de lignes éditables pré-remplies (identique à GENERAL_DATA_ROWS côté JS)
    private const int DataRows = 6;

    // Nombre de lignes couvertes par les listes déroulantes (identique à VALIDATION_ROW_SPAN côté JS)
    private const int ValidationRowSpan = 300;

    public ExportTemplateService(VectisDbContext db)
    {
        _db = db;
    }

    // ========================================================================
    // 1) Définitions des colonnes — copiées à l'identique du prototype HTML
    // ========================================================================

    private static readonly string[] GeneralColumns =
    {
        "N° Dépense(obligatoire:Oui)", "Type dépense(obligatoire:Oui)", "Exercice(obligatoire:Oui)",
        "Type prestation", "Nature prestation(obligatoire:Oui)", "Type budget",
        "Objet(obligatoire:Oui)", "Observation", "Fournisseur(obligatoire:Oui)",
        "Représentant", "Centre Gestionnaire(obligatoire:Oui)", "Maître d'ouvrage",
        "Date de Dépense(obligatoire:Oui)", "Centre bénéficiaire(obligatoire:Oui)",
        "Devise(obligatoire:Oui)", "Chargé de suivi-Matricule et nom et prenom",
        "Chargé de suivi-Mission(obligatoire:Oui si chargé de suivi)",
        "Délai d'exécution(obligatoire:Oui)", "Délai de garantie",
        "Mode de paiement(obligatoire:Oui)", "Banque(obligatoire: Oui si Virement)",
        "N° compte(obligatoire: Oui si Virement)", "Personne Habilitée(obligatoire:Oui si BC)",
        "Taux de remise", "Taux de retenue source",
        "Validation(obligatoire:Oui)", "Date de validation(obligatoire:Oui si Validation)"
    };

    private static readonly string[] BorderauxColumns =
    {
        "N° Dépense(obligatoire:Oui)",
        "Désignation Produit(obligatoire:Oui)",
        "Quantité(obligatoire:Oui)", "PU minimum(obligatoire:Oui)",
        "Prix Unitaire(obligatoire:Oui)", "TVA", "Remise",
        "Prix révisable", "RS", "RS sur TVA"
    };

    private static readonly string[] OrdreServiceColumns =
    {
        "N° Dépense(obligatoire:Oui)",
        "N° Ordre de service(obligatoire:Oui)",
        "Date OS(obligatoire:Oui)",
        "Date Notification OS(obligatoire:Oui)",
        "Date d'effet OS(obligatoire:Oui)"
    };

    private sealed record StepDef(string Key, string Name, bool HasValidation, string[] Columns);

    private static readonly StepDef[] StepDefs =
    {
        new("engagement", "Engagement", true, new[]
        {
            "N° dépense(obligatoire:Oui)", "Catégorie Budget(obligatoire:Oui)",
            "Date engagement(obligatoire:Oui)",
            "Lignes budgétaires-Code ligne budgétaire(obligatoire:Oui)",
            "Montant à engager CP(obligatoire:Oui)", "Montant à engager CE",
            "Validation(obligatoire:Oui)", "Date de validation(obligatoire:Oui si Validation)"
        }),
        new("livraison", "Livraison", true, new[]
        {
            "N° Dépense(obligatoire:Oui)", "N° BL(obligatoire:Oui)", "Responsable livraison",
            "Magasin", "Magasinier", "Date livraison(obligatoire:Oui)",
            "Heure de livraison", "Date livraison prévisionnelle",
            "Date dépôt", "Produit-N° Prix(obligatoire:Oui)",
            "Produit-Quantité Livrée(obligatoire:Oui)", "Produit-Quantité acceptée n",
            "Validation(obligatoire:Oui)", "Date de validation(obligatoire:Oui si Validation)"
        }),
        new("reception", "Réception", true, new[]
        {
            "N° Dépense(obligatoire:Oui)",
            "Date réception(obligatoire:Oui)", "Nature réception(obligatoire:Oui)",
            "Type réception(obligatoire:Oui)", "Lieux", "Numéro fiche réception",
            "Heure réception", "N° Livraison(obligatoire:Oui)",
            "Validation(obligatoire:Oui)", "Date de validation(obligatoire:Oui si Validation)"
        }),
        new("facture", "Facture", true, new[]
        {
            "N° Dépense(obligatoire:Oui)",
            "N° facture fournisseur(obligatoire:Oui)", "Date facture(obligatoire:Oui)",
            "Date dépôt facture(obligatoire:Oui)", "Période", "Montant Révision",
            "Calculer RG TOTAL", "Calculer les pénalités de retard", "% Reception", "% Marché",
            "Taux RS sur TVA Appliqué", "Pénalité Spéciale", "Pénalité Spéciale-Nombre de jour de manquement",
            "Pénalité Spéciale-Indemnisation", "Observation", "Justification",
            "N° Livraison(obligatoire:Oui)","Validation(obligatoire:Oui)", "Date de validation(obligatoire:Oui si Validation)"
        }),
    };

    // ========================================================================
    // 2) Liens colonne -> source de liste déroulante (copiés de referentielLinks)
    //    - RefColumn renseigné  => la source est la feuille "Référentiel"
    //    - SourceSheetKey/SourceColumn renseignés => la source est une autre feuille
    //      (ex: N° Dépense de "Borderaux de prix" pointe vers "Fiche Générale")
    // ========================================================================

    private sealed record RefLink(string TargetSheetKey, string TargetColumn, string? RefColumn, string? SourceSheetKey, string? SourceColumn)
    {
        public static RefLink FromReferentiel(string targetSheetKey, string targetColumn, string refColumn)
            => new(targetSheetKey, targetColumn, refColumn, null, null);

        public static RefLink FromSheet(string targetSheetKey, string targetColumn, string sourceSheetKey, string sourceColumn)
            => new(targetSheetKey, targetColumn, null, sourceSheetKey, sourceColumn);
    }

    private static readonly RefLink[] RefLinks =
    {
        RefLink.FromReferentiel("general", "Type dépense(obligatoire:Oui)", "Type dépense(DEP_TYPE_CONTRAT)"),
        RefLink.FromReferentiel("general", "Exercice(obligatoire:Oui)", "Exercice(BDG_EXERCICE)"),
        RefLink.FromReferentiel("general", "Type prestation", "Type prestation(DEP_TYPE_PRESTATION)"),
        RefLink.FromReferentiel("general", "Type budget", "Type budget(BDG_TYPE_BUDGET)"),
        RefLink.FromReferentiel("general", "Fournisseur(obligatoire:Oui)", "Fournisseur(GEN_FOURNISSEUR)"),
        RefLink.FromReferentiel("general", "Représentant", "Représentant"),
        RefLink.FromReferentiel("general", "Centre Gestionnaire(obligatoire:Oui)", "Centre Gestionnaire(GEN_CENTRE_RESPONSABILITE join GEN_CENTRE_RESPONSABILITE_H)"),
        RefLink.FromReferentiel("general", "Maître d'ouvrage", "Maître d'ouvrage(BDG_MAITRE_OUVRAGE)"),
        RefLink.FromReferentiel("general", "Centre bénéficiaire(obligatoire:Oui)", "Centre Gestionnaire(GEN_CENTRE_RESPONSABILITE join GEN_CENTRE_RESPONSABILITE_H)"),
        RefLink.FromReferentiel("general", "Devise(obligatoire:Oui)", "Devise(GEN_DEVISE)"),
        RefLink.FromReferentiel("general", "Chargé de suivi-Matricule et nom et prenom", "Chargé de suivi-Matricule et nom et prenom(ref_agent)"),
        RefLink.FromReferentiel("general", "Mode de paiement(obligatoire:Oui)", "Mode de paiement(GEN_TYPE_REGLEMENT)"),
        RefLink.FromReferentiel("general", "Personne Habilitée(obligatoire:Oui si BC)", "Personne Habilitée(ref_agent)"),
        RefLink.FromReferentiel("general", "Validation(obligatoire:Oui)", "Binaire"),

        RefLink.FromSheet("borderaux", "N° Dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),
        RefLink.FromReferentiel("borderaux", "Désignation Produit(obligatoire:Oui)", "Désignation Produit(GEN_PRODUIT join GEN_PRODUIT_BRD_1)"),
        RefLink.FromReferentiel("borderaux", "Remise", "Binaire"),
        RefLink.FromReferentiel("borderaux", "Prix révisable", "Binaire"),
        RefLink.FromReferentiel("borderaux", "RS", "Binaire"),
        RefLink.FromReferentiel("borderaux", "RS sur TVA", "Binaire"),

        RefLink.FromSheet("ordreService", "N° Dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),

        RefLink.FromSheet("engagement", "N° dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),
        RefLink.FromReferentiel("engagement", "Catégorie Budget(obligatoire:Oui)", "Catégorie Budget(CPT_TYP_CREDIT_SUPP)"),
        RefLink.FromReferentiel("engagement", "Validation(obligatoire:Oui)", "Binaire"),

        RefLink.FromSheet("livraison", "N° Dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),
        RefLink.FromReferentiel("livraison", "Responsable livraison", "Responsable livraison(ref_agent join GEN_CENTRE_RESP_CONTACTS)"),
        RefLink.FromReferentiel("livraison", "Magasin", "Magasin(GEN_MAGASIN)"),
        RefLink.FromReferentiel("livraison", "Magasinier", "Magasinier(stk_magasinier join STK_MAGASIN_MAGASINIER)"),
        RefLink.FromReferentiel("livraison", "Validation(obligatoire:Oui)", "Binaire"),

        RefLink.FromSheet("reception", "N° Dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),
        RefLink.FromReferentiel("reception", "Nature réception(obligatoire:Oui)", "Nature réception(DEP_NATURE_RECEPTION)"),
        RefLink.FromReferentiel("reception", "Type réception(obligatoire:Oui)", "Type réception(DEP_TYPE_RECEPTION)"),
        RefLink.FromReferentiel("reception", "Validation(obligatoire:Oui)", "Binaire"),

        RefLink.FromSheet("facture", "N° Dépense(obligatoire:Oui)", "general", "N° Dépense(obligatoire:Oui)"),
        RefLink.FromReferentiel("facture", "Période", "Période"),
        RefLink.FromReferentiel("facture", "Calculer RG TOTAL", "Binaire"),
        RefLink.FromReferentiel("facture", "Calculer les pénalités de retard", "Binaire"),
        RefLink.FromReferentiel("facture", "% Reception", "Binaire"),
        RefLink.FromReferentiel("facture", "% Marché", "Binaire"),
        RefLink.FromReferentiel("facture", "Pénalité Spéciale", "Binaire"),
        RefLink.FromReferentiel("facture", "Validation(obligatoire:Oui)", "Binaire"),
    };

    // ========================================================================
    // 3) Génération du classeur
    // ========================================================================

    public async Task<byte[]> BuildImportTemplateAsync(ExportOptions options, CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();

        // Étapes sélectionnées côté front (équivalent de includedSteps en JS)
        var selectedSteps = StepDefs
            .Select(def => new
            {
                Def = def,
                Selection = options.Steps.FirstOrDefault(s => s.Key == def.Key)
            })
            .Where(x => x.Selection is { Included: true })
            .ToList();

        // Registre feuille -> (worksheet, en-têtes) pour résoudre les colonnes cibles/sources
        var registry = new Dictionary<string, (IXLWorksheet Ws, string[] Headers)>();

        // ---- 1) Feuilles fixes : Fiche Générale / Borderaux de prix / Ordre de service ----
        var wsGeneral = wb.Worksheets.Add("Fiche Générale");
        BuildSimpleSheet(wsGeneral, GeneralColumns);
        registry["general"] = (wsGeneral, GeneralColumns);

        var wsBord = wb.Worksheets.Add("Borderaux de prix");
        BuildSimpleSheet(wsBord, BorderauxColumns);
        registry["borderaux"] = (wsBord, BorderauxColumns);

        var wsOs = wb.Worksheets.Add("Ordre de service");
        BuildSimpleSheet(wsOs, OrdreServiceColumns);
        registry["ordreService"] = (wsOs, OrdreServiceColumns);

        // ---- 2) Une feuille par étape sélectionnée (Engagement / Livraison / Réception / Facture) ----
        foreach (var s in selectedSteps)
        {
            var def = s.Def;
            var avecValidation = s.Selection!.AvecValidation;

            var columns = def.Columns;

            var ws = wb.Worksheets.Add(Truncate(def.Name, 31));
            WriteHeaderRow(ws, columns);
            AutoWidth(ws, columns);

            for (int i = 0; i < DataRows; i++)
            {
                var rowIdx = 2 + i;
                if (def.HasValidation)
                    ws.Cell(rowIdx, columns.Length - 1).Value = avecValidation ? "Oui" : "Non";
            }

            registry[def.Key] = (ws, columns);
        }

        // ---- 3) Feuille "Référentiel" (orientation colonnes, sert de source aux listes) ----
        var referentiel = await BuildReferentielDataAsync(ct);
        var wsRef = wb.Worksheets.Add("Référentiel");
        var refColumns = referentiel.Keys.ToArray();

        for (int c = 0; c < refColumns.Length; c++)
            wsRef.Cell(1, c + 1).Value = refColumns[c];
        StyleHeaderNoColor(wsRef.Range(1, 1, 1, refColumns.Length));

        for (int c = 0; c < refColumns.Length; c++)
        {
            var values = referentiel[refColumns[c]];
            for (int r = 0; r < values.Count; r++)
                wsRef.Cell(2 + r, c + 1).Value = values[r];
        }
        AutoWidth(wsRef, refColumns);
        registry["referentiel"] = (wsRef, refColumns);

        // ---- 3bis) Colonnes d'aide pour la liste "Banque" en cascade (dépendante du Fournisseur) ----
        // Placées juste après les colonnes du référentiel normal, triées par fournisseur pour
        // que la formule OFFSET/MATCH/COUNTIF puisse extraire un bloc contigu par fournisseur.
        var cascadeFournisseurBanque = await BuildFournisseurBanqueCascadeAsync(ct);
        var cascadeFournisseurCol = refColumns.Length + 1;
        var cascadeBanqueCol = refColumns.Length + 2;

        wsRef.Cell(1, cascadeFournisseurCol).Value = "Cascade Fournisseur→Banque : Fournisseur";
        wsRef.Cell(1, cascadeBanqueCol).Value = "Cascade Fournisseur→Banque : Banque";
        StyleHeaderNoColor(wsRef.Range(1, cascadeFournisseurCol, 1, cascadeBanqueCol));

        for (int r = 0; r < cascadeFournisseurBanque.Count; r++)
        {
            wsRef.Cell(2 + r, cascadeFournisseurCol).Value = cascadeFournisseurBanque[r].Fournisseur;
            wsRef.Cell(2 + r, cascadeBanqueCol).Value = cascadeFournisseurBanque[r].Banque;
        }
        wsRef.Column(cascadeFournisseurCol).Width = 28;
        wsRef.Column(cascadeBanqueCol).Width = 28;

        // ---- 3ter) Colonnes d'aide pour la liste "N° Compte" en cascade (dépendante du Fournisseur) ----
        var cascadeFournisseurCompte = await BuildFournisseurCompteCascadeAsync(ct);
        var cascadeFournisseurCompteCol = refColumns.Length + 3;
        var cascadeCompteCol = refColumns.Length + 4;

        wsRef.Cell(1, cascadeFournisseurCompteCol).Value = "Cascade Fournisseur→N° Compte : Fournisseur";
        wsRef.Cell(1, cascadeCompteCol).Value = "Cascade Fournisseur→N° Compte : N° Compte";
        StyleHeaderNoColor(wsRef.Range(1, cascadeFournisseurCompteCol, 1, cascadeCompteCol));

        for (int r = 0; r < cascadeFournisseurCompte.Count; r++)
        {
            wsRef.Cell(2 + r, cascadeFournisseurCompteCol).Value = cascadeFournisseurCompte[r].Fournisseur;
            wsRef.Cell(2 + r, cascadeCompteCol).Value = cascadeFournisseurCompte[r].NumCompte;
        }
        wsRef.Column(cascadeFournisseurCompteCol).Width = 28;
        wsRef.Column(cascadeCompteCol).Width = 28;

        // ---- 3quater) Colonnes d'aide pour "Nature prestation" en cascade DOUBLE
        //      (dépend de Type dépense ET Type prestation choisis sur la même ligne) ----
        var cascadeNature = await BuildTypeDepensePrestationNatureCascadeAsync(ct);
        var cascadeTypeDepenseCol = refColumns.Length + 5;
        var cascadeTypePrestationCol = refColumns.Length + 6;
        var cascadeNatureCol = refColumns.Length + 7;
        var cascadeNatureCleCol = refColumns.Length + 8;

        wsRef.Cell(1, cascadeTypeDepenseCol).Value = "Cascade TypeDépense+TypePrestation→Nature : Type dépense";
        wsRef.Cell(1, cascadeTypePrestationCol).Value = "Cascade TypeDépense+TypePrestation→Nature : Type prestation";
        wsRef.Cell(1, cascadeNatureCol).Value = "Cascade TypeDépense+TypePrestation→Nature : Nature";
        wsRef.Cell(1, cascadeNatureCleCol).Value = "Cascade TypeDépense+TypePrestation→Nature : Clé";
        StyleHeaderNoColor(wsRef.Range(1, cascadeTypeDepenseCol, 1, cascadeNatureCleCol));

        for (int r = 0; r < cascadeNature.Count; r++)
        {
            var row = 2 + r;
            wsRef.Cell(row, cascadeTypeDepenseCol).Value = cascadeNature[r].TypeDepense;
            wsRef.Cell(row, cascadeTypePrestationCol).Value = cascadeNature[r].TypePrestation;
            wsRef.Cell(row, cascadeNatureCol).Value = cascadeNature[r].Nature;
            // Clé composite = concaténation des deux colonnes de tri (même valeur, même séparateur "|"
            // que celle reconstruite par la formule Excel côté Fiche Générale).
            wsRef.Cell(row, cascadeNatureCleCol).Value = cascadeNature[r].TypeDepense + "|" + cascadeNature[r].TypePrestation;
        }
        wsRef.Column(cascadeTypeDepenseCol).Width = 24;
        wsRef.Column(cascadeTypePrestationCol).Width = 24;
        wsRef.Column(cascadeNatureCol).Width = 24;
        wsRef.Column(cascadeNatureCleCol).Width = 30;

        // ---- 3quinquies) Colonnes d'aide pour "Lignes budgétaires" en cascade (dépend de
        //      l'Exercice choisi dans la Fiche Générale pour le N° dépense de la ligne) ----
        var cascadeLigneBudgetaire = await BuildExerciceLigneBudgetaireCascadeAsync(ct);
        var cascadeExerciceLigneCol = refColumns.Length + 9;
        var cascadeLigneBudgetaireCol = refColumns.Length + 10;

        wsRef.Cell(1, cascadeExerciceLigneCol).Value = "Cascade Exercice→LigneBudgétaire : Exercice";
        wsRef.Cell(1, cascadeLigneBudgetaireCol).Value = "Cascade Exercice→LigneBudgétaire : Ligne budgétaire";
        StyleHeaderNoColor(wsRef.Range(1, cascadeExerciceLigneCol, 1, cascadeLigneBudgetaireCol));

        for (int r = 0; r < cascadeLigneBudgetaire.Count; r++)
        {
            wsRef.Cell(2 + r, cascadeExerciceLigneCol).Value = cascadeLigneBudgetaire[r].Exercice;
            wsRef.Cell(2 + r, cascadeLigneBudgetaireCol).Value = cascadeLigneBudgetaire[r].LigneBudgetaire;
        }
        wsRef.Column(cascadeExerciceLigneCol).Width = 20;
        wsRef.Column(cascadeLigneBudgetaireCol).Width = 40;

        // ---- 4) Application des listes déroulantes (data validation) ----
        foreach (var link in RefLinks)
        {
            if (!registry.TryGetValue(link.TargetSheetKey, out var target)) continue;

            var targetColIndex = Array.IndexOf(target.Headers, link.TargetColumn) + 1;
            if (targetColIndex == 0) continue;

            IXLRange? sourceRange = null;

            if (link.RefColumn != null)
            {
                var refColIndex = Array.IndexOf(refColumns, link.RefColumn) + 1;
                if (refColIndex > 0)
                    sourceRange = wsRef.Range(2, refColIndex, ValidationRowSpan + 1, refColIndex);
            }
            else if (link.SourceSheetKey != null && link.SourceColumn != null
                     && registry.TryGetValue(link.SourceSheetKey, out var source))
            {
                var srcColIndex = Array.IndexOf(source.Headers, link.SourceColumn) + 1;
                if (srcColIndex > 0)
                    sourceRange = source.Ws.Range(2, srcColIndex, ValidationRowSpan + 1, srcColIndex);
            }

            if (sourceRange == null) continue;

            // ---- Validation (liste déroulante) ----
            var targetRange = target.Ws.Range(2, targetColIndex, ValidationRowSpan + 1, targetColIndex);
            var dv = targetRange.CreateDataValidation();
            dv.List(sourceRange, true);
            dv.IgnoreBlanks = true;
            dv.InCellDropdown = true;
            dv.ShowErrorMessage = true;
            dv.ErrorStyle = XLErrorStyle.Warning;
            dv.ErrorTitle = "Valeur hors référentiel";
            dv.ErrorMessage = "Cette valeur ne figure pas dans la liste autorisée. Vous pouvez la saisir librement ou compléter le référentiel.";
            dv.ShowInputMessage = true;
            dv.InputTitle = "Liste déroulante";
            dv.InputMessage = "Cliquez sur la flèche pour choisir une valeur, ou saisissez-en une nouvelle.";

            // ---- Design : mise en valeur visuelle des cellules éditables avec liste ----
            var editableRange = target.Ws.Range(2, targetColIndex, DataRows + 1, targetColIndex);
            editableRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC); // bleu très clair
            editableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            editableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            editableRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
            editableRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
            editableRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
        }

        // ---- 4bis) Liste déroulante en cascade "Banque" (dépend du Fournisseur de la ligne) ----
        // Technique OFFSET/MATCH/COUNTIF : la plage "Cascade Fournisseur→Banque" du Référentiel
        // est triée par fournisseur (blocs contigus), donc COUNTIF donne la taille du bloc et
        // MATCH son point de départ. IFERROR bascule sur la liste complète de banques tant
        // qu'aucun fournisseur n'est encore choisi sur la ligne (ou si aucune correspondance).
        {
            var fournisseurColIdx = Array.IndexOf(GeneralColumns, "Fournisseur(obligatoire:Oui)") + 1;
            var banqueColIdx = Array.IndexOf(GeneralColumns, "Banque(obligatoire: Oui si Virement)") + 1;

            if (fournisseurColIdx > 0 && banqueColIdx > 0 && cascadeFournisseurBanque.Count > 0)
            {
                var fournisseurColLetter = ColumnLetter(fournisseurColIdx);
                var cascadeFournisseurColLetter = ColumnLetter(cascadeFournisseurCol);
                var cascadeBanqueColLetter = ColumnLetter(cascadeBanqueCol);
                var cascadeLastRow = 1 + cascadeFournisseurBanque.Count;

                // NB : PAS de IFERROR() ici — envelopper un OFFSET() qui renvoie une PLAGE dans
                // IFERROR() est un cas connu où le moteur de validation de données d'Excel
                // (différent de celui des formules de cellule normales) échoue silencieusement
                // et affiche une liste vide, même quand la correspondance existe. Sans Fournisseur
                // choisi sur la ligne, la liste reste simplement vide (comportement standard
                // d'une liste en cascade), au lieu de retomber sur la liste complète.
                var formula =
                    "=OFFSET('Référentiel'!$" + cascadeBanqueColLetter + "$2," +
                    "MATCH($" + fournisseurColLetter + "2,'Référentiel'!$" + cascadeFournisseurColLetter + "$2:$" + cascadeFournisseurColLetter + "$" + cascadeLastRow + ",0)-1,0," +
                    "COUNTIF('Référentiel'!$" + cascadeFournisseurColLetter + "$2:$" + cascadeFournisseurColLetter + "$" + cascadeLastRow + ",$" + fournisseurColLetter + "2),1)";

                var banqueRange = wsGeneral.Range(2, banqueColIdx, ValidationRowSpan + 1, banqueColIdx);
                var dvBanque = banqueRange.CreateDataValidation();
                dvBanque.List(formula, true);
                dvBanque.IgnoreBlanks = true;
                dvBanque.InCellDropdown = true;
                dvBanque.ShowErrorMessage = true;
                dvBanque.ErrorStyle = XLErrorStyle.Warning;
                dvBanque.ErrorTitle = "Banque hors référentiel du fournisseur";
                dvBanque.ErrorMessage = "Cette banque ne correspond pas au fournisseur sélectionné sur cette ligne. Vous pouvez la saisir librement.";
                dvBanque.ShowInputMessage = true;
                dvBanque.InputTitle = "Banque du fournisseur";
                dvBanque.InputMessage = "La liste dépend du fournisseur choisi sur cette ligne (choisissez d'abord le fournisseur).";

                var editableBanqueRange = wsGeneral.Range(2, banqueColIdx, DataRows + 1, banqueColIdx);
                editableBanqueRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableBanqueRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableBanqueRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableBanqueRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableBanqueRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableBanqueRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4ter) Liste déroulante en cascade "N° Compte" (dépend du Fournisseur de la ligne) ----
        // Même technique OFFSET/MATCH/COUNTIF que pour la Banque, sur la zone d'aide
        // "Cascade Fournisseur→N° Compte" du Référentiel.
        {
            var fournisseurColIdx = Array.IndexOf(GeneralColumns, "Fournisseur(obligatoire:Oui)") + 1;
            var compteColIdx = Array.IndexOf(GeneralColumns, "N° compte(obligatoire: Oui si Virement)") + 1;

            if (fournisseurColIdx > 0 && compteColIdx > 0 && cascadeFournisseurCompte.Count > 0)
            {
                var fournisseurColLetter = ColumnLetter(fournisseurColIdx);
                var cascadeFournisseurCompteColLetter = ColumnLetter(cascadeFournisseurCompteCol);
                var cascadeCompteColLetter = ColumnLetter(cascadeCompteCol);
                var cascadeCompteLastRow = 1 + cascadeFournisseurCompte.Count;

                var formula =
                    "=OFFSET('Référentiel'!$" + cascadeCompteColLetter + "$2," +
                    "MATCH($" + fournisseurColLetter + "2,'Référentiel'!$" + cascadeFournisseurCompteColLetter + "$2:$" + cascadeFournisseurCompteColLetter + "$" + cascadeCompteLastRow + ",0)-1,0," +
                    "COUNTIF('Référentiel'!$" + cascadeFournisseurCompteColLetter + "$2:$" + cascadeFournisseurCompteColLetter + "$" + cascadeCompteLastRow + ",$" + fournisseurColLetter + "2),1)";

                var compteRange = wsGeneral.Range(2, compteColIdx, ValidationRowSpan + 1, compteColIdx);
                var dvCompte = compteRange.CreateDataValidation();
                dvCompte.List(formula, true);
                dvCompte.IgnoreBlanks = true;
                dvCompte.InCellDropdown = true;
                dvCompte.ShowErrorMessage = true;
                dvCompte.ErrorStyle = XLErrorStyle.Warning;
                dvCompte.ErrorTitle = "N° compte hors référentiel du fournisseur";
                dvCompte.ErrorMessage = "Ce numéro de compte ne correspond pas au fournisseur sélectionné sur cette ligne. Vous pouvez le saisir librement.";
                dvCompte.ShowInputMessage = true;
                dvCompte.InputTitle = "N° Compte du fournisseur";
                dvCompte.InputMessage = "La liste dépend du fournisseur choisi sur cette ligne (choisissez d'abord le fournisseur).";

                var editableCompteRange = wsGeneral.Range(2, compteColIdx, DataRows + 1, compteColIdx);
                editableCompteRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableCompteRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableCompteRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableCompteRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableCompteRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableCompteRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4quater) Liste déroulante en cascade "Nature prestation" (dépend de Type dépense
        //      ET Type prestation choisis sur la même ligne) ----
        // Clé composite : $<TypeDépenseCol>2&"|"&$<TypePrestationCol>2, comparée à la colonne
        // "Clé" du bloc d'aide (même construction, même séparateur "|").
        {
            var typeDepenseColIdx = Array.IndexOf(GeneralColumns, "Type dépense(obligatoire:Oui)") + 1;
            var typePrestationColIdx = Array.IndexOf(GeneralColumns, "Type prestation") + 1;
            var natureColIdx = Array.IndexOf(GeneralColumns, "Nature prestation(obligatoire:Oui)") + 1;

            if (typeDepenseColIdx > 0 && typePrestationColIdx > 0 && natureColIdx > 0 && cascadeNature.Count > 0)
            {
                var typeDepenseColLetter = ColumnLetter(typeDepenseColIdx);
                var typePrestationColLetter = ColumnLetter(typePrestationColIdx);
                var cascadeNatureColLetter = ColumnLetter(cascadeNatureCol);
                var cascadeNatureCleColLetter = ColumnLetter(cascadeNatureCleCol);
                var cascadeNatureLastRow = 1 + cascadeNature.Count;

                // Clé composite reconstruite côté Fiche Générale, à partir des deux colonnes de la ligne
                var cleLigne = "$" + typeDepenseColLetter + "2&\"|\"&$" + typePrestationColLetter + "2";

                var formula =
                    "=OFFSET('Référentiel'!$" + cascadeNatureColLetter + "$2," +
                    "MATCH(" + cleLigne + ",'Référentiel'!$" + cascadeNatureCleColLetter + "$2:$" + cascadeNatureCleColLetter + "$" + cascadeNatureLastRow + ",0)-1,0," +
                    "COUNTIF('Référentiel'!$" + cascadeNatureCleColLetter + "$2:$" + cascadeNatureCleColLetter + "$" + cascadeNatureLastRow + "," + cleLigne + "),1)";

                var natureRange = wsGeneral.Range(2, natureColIdx, ValidationRowSpan + 1, natureColIdx);
                var dvNature = natureRange.CreateDataValidation();
                dvNature.List(formula, true);
                dvNature.IgnoreBlanks = true;
                dvNature.InCellDropdown = true;
                dvNature.ShowErrorMessage = true;
                dvNature.ErrorStyle = XLErrorStyle.Warning;
                dvNature.ErrorTitle = "Nature hors référentiel";
                dvNature.ErrorMessage = "Cette nature de prestation ne correspond pas au Type dépense / Type prestation choisis sur cette ligne. Vous pouvez la saisir librement.";
                dvNature.ShowInputMessage = true;
                dvNature.InputTitle = "Nature de prestation";
                dvNature.InputMessage = "La liste dépend du Type dépense et du Type prestation choisis sur cette ligne (choisissez-les d'abord).";

                var editableNatureRange = wsGeneral.Range(2, natureColIdx, DataRows + 1, natureColIdx);
                editableNatureRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableNatureRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableNatureRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableNatureRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableNatureRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableNatureRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4quinquies) Liste déroulante en cascade "Lignes budgétaires" (feuille Engagement) ----
        // La clé (Exercice) n'est PAS sur la même feuille : on va la chercher dans la Fiche
        // Générale via VLOOKUP, à partir du N° dépense saisi sur la ligne Engagement.
        if (registry.TryGetValue("engagement", out var engagementSheet) && cascadeLigneBudgetaire.Count > 0)
        {
            var ndepEngColIdx = Array.IndexOf(engagementSheet.Headers, "N° dépense(obligatoire:Oui)") + 1;
            var ligneColIdx = Array.IndexOf(engagementSheet.Headers, "Lignes budgétaires-Code ligne budgétaire(obligatoire:Oui)") + 1;
            var ndepGenColIdx = Array.IndexOf(GeneralColumns, "N° Dépense(obligatoire:Oui)") + 1;
            var exerciceGenColIdx = Array.IndexOf(GeneralColumns, "Exercice(obligatoire:Oui)") + 1;

            if (ndepEngColIdx > 0 && ligneColIdx > 0 && ndepGenColIdx > 0 && exerciceGenColIdx > 0)
            {
                var ndepEngColLetter = ColumnLetter(ndepEngColIdx);
                var ndepGenColLetter = ColumnLetter(ndepGenColIdx);
                var exerciceGenColLetter = ColumnLetter(exerciceGenColIdx);
                var cascadeExerciceLigneColLetter = ColumnLetter(cascadeExerciceLigneCol);
                var cascadeLigneBudgetaireColLetter = ColumnLetter(cascadeLigneBudgetaireCol);
                var cascadeLigneLastRow = 1 + cascadeLigneBudgetaire.Count;
                // Position de la colonne "Exercice" par rapport à "N° Dépense" dans Fiche Générale (pour VLOOKUP)
                var vlookupColOffset = exerciceGenColIdx - ndepGenColIdx + 1;

                // Retrouve l'Exercice du N° dépense de cette ligne Engagement en le cherchant dans la Fiche Générale
                var exerciceLookup =
                    "VLOOKUP($" + ndepEngColLetter + "2,'Fiche Générale'!$" + ndepGenColLetter + "$2:$" + exerciceGenColLetter + "$" + (ValidationRowSpan + 1) + "," + vlookupColOffset + ",0)";

                var formula =
                    "=OFFSET('Référentiel'!$" + cascadeLigneBudgetaireColLetter + "$2," +
                    "MATCH(" + exerciceLookup + ",'Référentiel'!$" + cascadeExerciceLigneColLetter + "$2:$" + cascadeExerciceLigneColLetter + "$" + cascadeLigneLastRow + ",0)-1,0," +
                    "COUNTIF('Référentiel'!$" + cascadeExerciceLigneColLetter + "$2:$" + cascadeExerciceLigneColLetter + "$" + cascadeLigneLastRow + "," + exerciceLookup + "),1)";

                var ligneRange = engagementSheet.Ws.Range(2, ligneColIdx, ValidationRowSpan + 1, ligneColIdx);
                var dvLigne = ligneRange.CreateDataValidation();
                dvLigne.List(formula, true);
                dvLigne.IgnoreBlanks = true;
                dvLigne.InCellDropdown = true;
                dvLigne.ShowErrorMessage = true;
                dvLigne.ErrorStyle = XLErrorStyle.Warning;
                dvLigne.ErrorTitle = "Ligne budgétaire hors référentiel";
                dvLigne.ErrorMessage = "Cette ligne budgétaire ne correspond pas à l'exercice du N° dépense choisi sur cette ligne. Vous pouvez la saisir librement.";
                dvLigne.ShowInputMessage = true;
                dvLigne.InputTitle = "Ligne budgétaire";
                dvLigne.InputMessage = "La liste dépend de l'Exercice du N° dépense choisi en Fiche Générale (renseignez-le d'abord).";

                var editableLigneRange = engagementSheet.Ws.Range(2, ligneColIdx, DataRows + 1, ligneColIdx);
                editableLigneRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableLigneRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableLigneRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableLigneRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLigneRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLigneRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4sexies) Liste déroulante en cascade "Produit" (feuille Livraison) ----
        // La liste dépend du N° Dépense choisi sur la même ligne et pointe directement 
        // vers la feuille "Borderaux de prix" remplie par l'utilisateur.
        if (registry.TryGetValue("livraison", out var livraisonSheet))
        {
            var ndepLivColIdx = Array.IndexOf(livraisonSheet.Headers, "N° Dépense(obligatoire:Oui)") + 1;
            var produitLivColIdx = Array.IndexOf(livraisonSheet.Headers, "Produit-N° Prix(obligatoire:Oui)") + 1;

            var ndepBrdColIdx = Array.IndexOf(BorderauxColumns, "N° Dépense(obligatoire:Oui)") + 1;
            var produitBrdColIdx = Array.IndexOf(BorderauxColumns, "Désignation Produit(obligatoire:Oui)") + 1;

            if (ndepLivColIdx > 0 && produitLivColIdx > 0 && ndepBrdColIdx > 0 && produitBrdColIdx > 0)
            {
                var ndepLivColLetter = ColumnLetter(ndepLivColIdx);
                var ndepBrdColLetter = ColumnLetter(ndepBrdColIdx);
                var produitBrdColLetter = ColumnLetter(produitBrdColIdx);

                // Formule dynamique pointant sur la feuille 'Borderaux de prix'
                var formula =
                    "=OFFSET('Borderaux de prix'!$" + produitBrdColLetter + "$2," +
                    "MATCH($" + ndepLivColLetter + "2,'Borderaux de prix'!$" + ndepBrdColLetter + "$2:$" + ndepBrdColLetter + "$" + (ValidationRowSpan + 1) + ",0)-1,0," +
                    "COUNTIF('Borderaux de prix'!$" + ndepBrdColLetter + "$2:$" + ndepBrdColLetter + "$" + (ValidationRowSpan + 1) + ",$" + ndepLivColLetter + "2),1)";

                var wsLiv = livraisonSheet.Ws;
                var produitRange = wsLiv.Range(2, produitLivColIdx, ValidationRowSpan + 1, produitLivColIdx);

                var dvProduit = produitRange.CreateDataValidation();
                dvProduit.List(formula, true);
                dvProduit.IgnoreBlanks = true;
                dvProduit.InCellDropdown = true;
                dvProduit.ShowErrorMessage = true;
                dvProduit.ErrorStyle = XLErrorStyle.Warning;
                dvProduit.ErrorTitle = "Produit hors bordereau";
                dvProduit.ErrorMessage = "Ce produit ne figure pas dans la feuille 'Borderaux de prix' pour ce N° Dépense.";
                dvProduit.ShowInputMessage = true;
                dvProduit.InputTitle = "Produit du bordereau";
                dvProduit.InputMessage = "Choisissez un N° Dépense, puis sélectionnez un produit saisi dans l'onglet Borderaux de prix.";

                // Style visuel pour mettre en évidence les cellules éditables
                var editableProduitRange = wsLiv.Range(2, produitLivColIdx, DataRows + 1, produitLivColIdx);
                editableProduitRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableProduitRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableProduitRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableProduitRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableProduitRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableProduitRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4sevensies) Liste déroulante en cascade "N° Livraison / BL" (feuille Réception) ----
        // La liste dépend du N° Dépense choisi sur la même ligne et pointe directement 
        // vers la feuille "Livraison" remplie par l'utilisateur.
        if (registry.TryGetValue("reception", out var receptionSheet))
        {
            var ndepRecColIdx = Array.IndexOf(receptionSheet.Headers, "N° Dépense(obligatoire:Oui)") + 1;
            var livraisonRecColIdx = Array.IndexOf(receptionSheet.Headers, "N° Livraison(obligatoire:Oui)") + 1;

            var ndepLivColIdx = Array.IndexOf(livraisonSheet.Headers, "N° Dépense(obligatoire:Oui)") + 1;
            var nblLivColIdx = Array.IndexOf(livraisonSheet.Headers, "N° BL(obligatoire:Oui)") + 1;

            if (ndepRecColIdx > 0 && livraisonRecColIdx > 0 && ndepLivColIdx > 0 && nblLivColIdx > 0)
            {
                var ndepRecColLetter = ColumnLetter(ndepRecColIdx);
                var ndepLivColLetter = ColumnLetter(ndepLivColIdx);
                var nblLivColLetter = ColumnLetter(nblLivColIdx);

                // Formule dynamique pointant vers la feuille 'Livraison'
                var formula =
                    "=OFFSET('Livraison'!$" + nblLivColLetter + "$2," +
                    "MATCH($" + ndepRecColLetter + "2,'Livraison'!$" + ndepLivColLetter + "$2:$" + ndepLivColLetter + "$" + (ValidationRowSpan + 1) + ",0)-1,0," +
                    "COUNTIF('Livraison'!$" + ndepLivColLetter + "$2:$" + ndepLivColLetter + "$" + (ValidationRowSpan + 1) + ",$" + ndepRecColLetter + "2),1)";

                var wsRec = receptionSheet.Ws;
                var livraisonRange = wsRec.Range(2, livraisonRecColIdx, ValidationRowSpan + 1, livraisonRecColIdx);

                var dvLivraison = livraisonRange.CreateDataValidation();
                dvLivraison.List(formula, true);
                dvLivraison.IgnoreBlanks = true;
                dvLivraison.InCellDropdown = true;
                dvLivraison.ShowErrorMessage = true;
                dvLivraison.ErrorStyle = XLErrorStyle.Warning;
                dvLivraison.ErrorTitle = "Livraison non trouvée";
                dvLivraison.ErrorMessage = "Ce N° de livraison ne figure pas dans la feuille 'Livraison' pour ce N° Dépense.";
                dvLivraison.ShowInputMessage = true;
                dvLivraison.InputTitle = "N° Livraison / BL";
                dvLivraison.InputMessage = "Choisissez un N° Dépense, puis sélectionnez une livraison saisie dans l'onglet Livraison.";

                // Style visuel pour mettre en évidence les cellules éditables sur la feuille Réception
                var editableLivraisonRange = wsRec.Range(2, livraisonRecColIdx, DataRows + 1, livraisonRecColIdx);
                editableLivraisonRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableLivraisonRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableLivraisonRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableLivraisonRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLivraisonRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLivraisonRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 4eighties) Liste déroulante en cascade "N° Livraison / BL" (feuille Facture) ----
        // La liste dépend du N° Dépense choisi sur la même ligne et pointe directement 
        // vers la feuille "Livraison" remplie par l'utilisateur.
        if (registry.TryGetValue("facture", out var factureSheet))
        {
            var ndepFacColIdx = Array.IndexOf(factureSheet.Headers, "N° Dépense(obligatoire:Oui)") + 1;
            var livraisonFacColIdx = Array.IndexOf(factureSheet.Headers, "N° Livraison(obligatoire:Oui)") + 1;

            var ndepLivColIdx = Array.IndexOf(livraisonSheet.Headers, "N° Dépense(obligatoire:Oui)") + 1;
            var nblLivColIdx = Array.IndexOf(livraisonSheet.Headers, "N° BL(obligatoire:Oui)") + 1;

            if (ndepFacColIdx > 0 && livraisonFacColIdx > 0 && ndepLivColIdx > 0 && nblLivColIdx > 0)
            {
                var ndepFacColLetter = ColumnLetter(ndepFacColIdx);
                var ndepLivColLetter = ColumnLetter(ndepLivColIdx);
                var nblLivColLetter = ColumnLetter(nblLivColIdx);

                // Formule dynamique pointant vers la feuille 'Livraison'
                var formula =
                    "=OFFSET('Livraison'!$" + nblLivColLetter + "$2," +
                    "MATCH($" + ndepFacColLetter + "2,'Livraison'!$" + ndepLivColLetter + "$2:$" + ndepLivColLetter + "$" + (ValidationRowSpan + 1) + ",0)-1,0," +
                    "COUNTIF('Livraison'!$" + ndepLivColLetter + "$2:$" + ndepLivColLetter + "$" + (ValidationRowSpan + 1) + ",$" + ndepFacColLetter + "2),1)";

                var wsRec = factureSheet.Ws;
                var livraisonRange = wsRec.Range(2, livraisonFacColIdx, ValidationRowSpan + 1, livraisonFacColIdx);
                var dvLivraison = livraisonRange.CreateDataValidation();
                dvLivraison.List(formula, true);
                dvLivraison.IgnoreBlanks = true;
                dvLivraison.InCellDropdown = true;
                dvLivraison.ShowErrorMessage = true;
                dvLivraison.ErrorStyle = XLErrorStyle.Warning;
                dvLivraison.ErrorTitle = "Livraison non trouvée";
                dvLivraison.ErrorMessage = "Ce N° de livraison ne figure pas dans la feuille 'Livraison' pour ce N° Dépense.";
                dvLivraison.ShowInputMessage = true;
                dvLivraison.InputTitle = "N° Livraison / BL";
                dvLivraison.InputMessage = "Choisissez un N° Dépense, puis sélectionnez une livraison saisie dans l'onglet Livraison.";

                // Style visuel pour mettre en évidence les cellules éditables sur la feuille Réception
                var editableLivraisonRange = wsRec.Range(2, livraisonFacColIdx, DataRows + 1, livraisonFacColIdx);
                editableLivraisonRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEA, 0xF3, 0xFC);
                editableLivraisonRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                editableLivraisonRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                editableLivraisonRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLivraisonRange.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBB, 0xD6, 0xF0);
                editableLivraisonRange.Style.Font.FontColor = XLColor.FromArgb(0x1B, 0x4F, 0x8C);
            }
        }

        // ---- 5) Protection légère des feuilles (comme dans le prototype : en-têtes non modifiables) ----
        // Optionnel : décommenter si une protection est requise.
        // foreach (var ws in wb.Worksheets) ws.Protect().AllowElement(XLAllowedRange.SelectUnlockedCells);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ========================================================================
    // 4) Construction des données du référentiel (une colonne par rubrique)
    //    Les rubriques déjà couvertes par le code d'origine sont implémentées
    //    intégralement. Celles qui n'existaient pas dans le code fourni
    //    (Lignes budgétaires, Magasin, Magasinier, Responsable livraison,
    //    Période, Représentant) sont marquées // TODO : à brancher sur les
    //    entités EF réelles correspondantes.
    // ========================================================================

    private async Task<Dictionary<string, List<string>>> BuildReferentielDataAsync(CancellationToken ct)
    {
        // L'ordre des clés définit l'ordre des colonnes dans la feuille "Référentiel"
        // (identique à l'ordre de première apparition dans referentielLinks côté JS).
        var data = new Dictionary<string, List<string>>();

        data["Type dépense(DEP_TYPE_CONTRAT)"] = await _db.DepTypeContrats
            .Where(x => x.CodeTypeContrat == 1 || x.CodeTypeContrat == 2 || x.CodeTypeContrat == 4 || x.CodeTypeContrat == 5 || x.CodeTypeContrat == 49)
            .Select(x => x.LibelleTypeContrat!)
            .ToListAsync(ct);

        data["Exercice(BDG_EXERCICE)"] = await _db.BdgExercices
            .Where(x => x.Ouvert == 1 && x.Cloture == 0)
            .Select(x => x.Annee!.ToString())
            .ToListAsync(ct);

        data["Type prestation(DEP_TYPE_PRESTATION)"] = await _db.DepTypePrestations
            .Select(x => x.Designation!)
            .ToListAsync(ct);

        data["Nature prestation(DEP_NATURE_PRESTATION)"] = await _db.DepNaturePrestations
            .Select(x => x.LibelleNature!)
            .ToListAsync(ct);

        data["Type budget(BDG_TYPE_BUDGET)"] = await _db.BdgTypeBudgets
            .Select(x => x.DesignationTypeBdg!)
            .ToListAsync(ct);

        data["Fournisseur(GEN_FOURNISSEUR)"] = await _db.GenFournisseurs
            .Where(x => x.TypeFrs == 1 || x.TypeFrs == 2)
            .Select(x => x.LibelleFrs!)
            .ToListAsync(ct);

        // TODO : brancher sur la table/entité "Représentant" réelle si elle existe.
        data["Représentant"] = new List<string>();

        // Centre Gestionnaire / bénéficiaire
        data["Centre Gestionnaire(GEN_CENTRE_RESPONSABILITE join GEN_CENTRE_RESPONSABILITE_H)"] = await (
            from cr1 in _db.GenCentreResponsabilites
            where cr1.Bloquer == 0
            where _db.GenCentreResponsabiliteHs.Any(h => h.Ref == cr1.CodeCentre.ToString() && h.Type != "Racine")
            join cr2 in _db.GenCentreResponsabilites
                on cr1.IdSection equals cr2.CodeCentre.ToString() into cr2Group
            from cr2 in cr2Group.DefaultIfEmpty()
            orderby cr1.Designation
            select cr1.Designation + " - " + (cr2 != null ? (cr2.CentreId ?? "") : "")
        ).ToListAsync(ct);

        data["Maître d'ouvrage(BDG_MAITRE_OUVRAGE)"] = await _db.BdgMaitreOuvrages
            .Where(x => x.CodeMO == 2)
            .Select(x => ((x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim())
            .ToListAsync(ct);

        data["Devise(GEN_DEVISE)"] = await _db.GenDevises
            .Select(x => x.Devise!)
            .ToListAsync(ct);

        data["Chargé de suivi-Matricule et nom et prenom(ref_agent)"] = await _db.RefAgents
            .Select(x => ((x.Matricule ?? "") + " - " + (x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim())
            .ToListAsync(ct);

        data["Mode de paiement(GEN_TYPE_REGLEMENT)"] = await _db.GenTypeReglements
            .Where(x => x.CodeTypeReglement == 1 || x.CodeTypeReglement == 2)
            .Select(x => x.LibelleTypeReglement!)
            .ToListAsync(ct);

        // Banque(GEN_BANQUE) : liste GLOBALE de toutes les agences bancaires réellement
        var banqueAgencesToutesFournisseurs =
            from cb in _db.GenCompteBancaires
            join siege in _db.GenBanques
                on cb.CodeBanque equals siege.CodeBanque
            where siege.Siege == 1
            join agence in _db.GenBanques
                on siege.CodeBanque equals agence.CleSiege
            where agence.DesignationBanque == cb.Agence
                  && (agence.Siege == 0 || agence.Siege == null)
            select new
            {
                agence.CodeBanque,
                agence.DesignationBanque,
                Agence = cb.Agence
            };

        data["Banque(GEN_BANQUE)"] = await banqueAgencesToutesFournisseurs
            .Distinct()
            .Select(x => x.Agence == "A" ? x.DesignationBanque! : x.Agence!)
            .Distinct()
            .ToListAsync(ct);

        data["N° Compte(GEN_COMPTEBANCAIRE)"] = await _db.GenCompteBancaires
            .Select(x => x.NumCompte!)
            .ToListAsync(ct);

        data["Personne Habilitée(ref_agent)"] = await _db.RefAgents
            .Where(x => x.PersonneHabilitee == 1)
            .Select(x => ((x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim())
            .ToListAsync(ct);

        data["Désignation Produit(GEN_PRODUIT join GEN_PRODUIT_BRD_1)"] = await (
             from brd in _db.GenProduitBrds
             join prd in _db.GenProduits
                 on brd.CodeProduit equals prd.CodeProduit
             where brd.PrestationArchitecte == 0
                 && prd.ProduitGenerique != 1
                 && (prd.DateFermeture == null ||
                     prd.DateFermeture >= DateTime.Today)
                 && brd.Designation != null
             orderby brd.Designation
             select prd.ProduitId + " | " + prd.Designation
         ).ToListAsync(ct);

        data["Binaire"] = new List<string> { "Oui", "Non" };

        data["Catégorie Budget(CPT_TYP_CREDIT_SUPP)"] = await _db.CptTypeCreditSupps
            .Select(x => x.DesignationTypeCredit!)
            .ToListAsync(ct);

        // Lignes budgétaires : liste GLOBALE (tous exercices confondus). La cascade par
        // Exercice (voir BuildExerciceLigneBudgetaireCascadeAsync) applique les mêmes filtres
        // mais scopés à l'Annee, pour la colonne "Lignes budgétaires" de la feuille Engagement.
        data["Lignes budgétaires(BDG_STRUCTUREBUDGETAIRE_GEN_H)"] = await (
            from bdg in _db.BdgStructureBudgetaireGenHs
            where bdg.CategorieBudget != 1
                  && bdg.TypeCategorieBudget == 0
                  && bdg.TypeBudget == 1
                  && bdg.ActiveLigne == 1
                  && !string.IsNullOrEmpty(bdg.Codification)
                  && _db.BdgStructureBudgetaireGenHs
                        .Any(vbh => vbh.CodeLigneBdg == bdg.CodeLigneBdg && (vbh.Bop != 1 || vbh.Bop == null))
            orderby bdg.Codification
            select bdg.Codification + " - " + bdg.Libelle
        ).Distinct().ToListAsync(ct);

        data["Responsable livraison(ref_agent join GEN_CENTRE_RESP_CONTACTS)"] = await (
            from rf in _db.RefAgents
            join cr in _db.GenCentreRespContacts on rf.CodeAgent equals cr.CodeContact
            select ((rf.Nom ?? "") + " " + (rf.Prenom ?? "")).Trim()
            ).ToListAsync(ct);

        var magasinierCodes = await _db.StkMagasiniers
            .Where(sm => sm.Id == 1)
            .Select(sm => sm.CodeMagasinier)
            .ToListAsync(ct);

        // Requête principale
        data["Magasin(GEN_MAGASIN)"] = await (
            from gm in _db.GenMagasins
            join smm in _db.StkMagasinMagasiniers on gm.CodeMagasin equals (decimal?)smm.CodeMagasin
            join gv in _db.GenVilles on gm.CodeVille equals gv.CodeVille into villeGroup
            from gv in villeGroup.DefaultIfEmpty()
            where gm.MagasinReforme != 1
              && smm.CodeMagasinier.HasValue
              && magasinierCodes.Contains(smm.CodeMagasinier.Value)
            select ((gm.MagasinId ?? "") + " " + (gm.Libelle ?? "") + " " + (gv.DescriptionVille ?? "")).Trim())
            .Distinct()
            .OrderBy(libelle => libelle)
            .ToListAsync(ct);

        data["Magasinier(stk_magasinier join STK_MAGASIN_MAGASINIER)"] = await (
            from sm in _db.StkMagasiniers
            join smm in _db.StkMagasinMagasiniers on sm.CodeMagasinier equals (decimal?)smm.CodeMagasinier
            select ((sm.Nom + " " + sm.Prenom).Trim())
            ).ToListAsync(ct);

        data["Nature réception(DEP_NATURE_RECEPTION)"] = await _db.DepNatureReceptions
            .Select(x => x.LibelleNatureReception!)
            .ToListAsync(ct);

        data["Type réception(DEP_TYPE_RECEPTION)"] = await _db.DepTypeReceptions
            .Select(x => x.LibelleTypeReception!)
            .ToListAsync(ct);

        data["Période"] = new List<string>
        {
            "Janvier",
            "Février",
            "Mars",
            "Avril",
            "Mai",
            "Juin",
            "Juillet",
            "Août",
            "Septembre",
            "Octobre",
            "Novembre",
            "Décembre"
        };

        return data;
    }

    private async Task<List<(string Fournisseur, string Banque)>> BuildFournisseurBanqueCascadeAsync(CancellationToken ct)
    {
        var query =
            from cb in _db.GenCompteBancaires
            join frs in _db.GenFournisseurs on cb.CodeFrs equals frs.CodeFrs
            join siege in _db.GenBanques on cb.CodeBanque equals siege.CodeBanque
            where siege.Siege == 1
            join agence in _db.GenBanques on siege.CodeBanque equals agence.CleSiege
            where agence.DesignationBanque == cb.Agence
                  && (agence.Siege == 0 || agence.Siege == null)
            select new
            {
                Fournisseur = frs.LibelleFrs!,
                Banque = cb.Agence == "A" ? agence.DesignationBanque! : cb.Agence!
            };

        var pairs = await query.Distinct().ToListAsync(ct);

        // Tri par fournisseur obligatoire : la formule OFFSET/MATCH/COUNTIF utilisée pour la
        // cascade suppose que les lignes d'un même fournisseur sont contiguës.
        return pairs
            .OrderBy(x => x.Fournisseur, StringComparer.OrdinalIgnoreCase)
            .Select(x => (x.Fournisseur, x.Banque))
            .ToList();
    }

    private async Task<List<(string Fournisseur, string NumCompte)>> BuildFournisseurCompteCascadeAsync(CancellationToken ct)
    {
        var query =
            from cb in _db.GenCompteBancaires
            join frs in _db.GenFournisseurs on cb.CodeFrs equals frs.CodeFrs
            join siege in _db.GenBanques on cb.CodeBanque equals siege.CodeBanque
            where siege.Siege == 1
            join agence in _db.GenBanques on siege.CodeBanque equals agence.CleSiege
            where agence.DesignationBanque == cb.Agence
                  && (agence.Siege == 0 || agence.Siege == null)
            select new
            {
                Fournisseur = frs.LibelleFrs!,
                NumCompte = cb.NumCompte!
            };

        var pairs = await query.Distinct().ToListAsync(ct);

        // Même contrainte que pour la cascade Banque : tri obligatoire pour que les lignes
        // d'un même fournisseur soient contiguës (OFFSET/MATCH/COUNTIF).
        return pairs
            .OrderBy(x => x.Fournisseur, StringComparer.OrdinalIgnoreCase)
            .Select(x => (x.Fournisseur, x.NumCompte))
            .ToList();
    }

    private async Task<List<(string TypeDepense, string TypePrestation, string Nature)>> BuildTypeDepensePrestationNatureCascadeAsync(CancellationToken ct)
    {
        var query =
            from np in _db.DepNaturePrestations
            join td in _db.DepTypeContrats on np.CodeTypeDepense equals td.CodeTypeContrat
            join tp in _db.DepTypePrestations on np.CodeTypePrestation equals tp.CodeTypePrestation
            select new
            {
                TypeDepense = td.LibelleTypeContrat!,
                TypePrestation = tp.Designation!,
                Nature = np.LibelleNature!
            };

        var triples = await query.Distinct().ToListAsync(ct);

        // Tri par (Type dépense, Type prestation) obligatoire : la formule OFFSET/MATCH/COUNTIF
        // suppose que les lignes d'une même combinaison sont contiguës.
        return triples
            .OrderBy(x => x.TypeDepense, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.TypePrestation, StringComparer.OrdinalIgnoreCase)
            .Select(x => (x.TypeDepense, x.TypePrestation, x.Nature))
            .ToList();
    }

    /// <summary>
    /// Paires (Exercice, LigneBudgétaire) triées par Exercice, pour la liste déroulante en
    /// cascade "Lignes budgétaires" de la feuille Engagement. Reprend les mêmes filtres que la
    /// requête fournie (CategorieBudget &lt;&gt; 1, TypeCategorieBudget = 0, TypeBudget = 1,
    /// ActiveLigne = 1, Codification non vide, existence d'une ligne BOP &lt;&gt; 1), mais sans
    /// fixer l'Annee : on la garde comme clé de la cascade (Annee.ToString() = même format que
    /// la colonne "Exercice(BDG_EXERCICE)" du référentiel, qui affiche Annee.ToString()).
    /// </summary>
    private async Task<List<(string Exercice, string LigneBudgetaire)>> BuildExerciceLigneBudgetaireCascadeAsync(CancellationToken ct)
    {
        var query =
            from bdg in _db.BdgStructureBudgetaireGenHs
            where bdg.CategorieBudget != 1
                  && bdg.TypeCategorieBudget == 0
                  && bdg.TypeBudget == 1
                  && bdg.ActiveLigne == 1
                  && !string.IsNullOrEmpty(bdg.Codification)
                  && _db.BdgStructureBudgetaireGenHs
                        .Any(vbh => vbh.CodeLigneBdg == bdg.CodeLigneBdg && (vbh.Bop != 1 || vbh.Bop == null))
            select new
            {
                Exercice = bdg.Annee,
                LigneBudgetaire = bdg.Codification + " - " + bdg.Libelle
            };

        var pairs = await query.Distinct().ToListAsync(ct);

        // Tri par Exercice obligatoire : la formule OFFSET/MATCH/COUNTIF suppose que les lignes
        // d'un même exercice sont contiguës.
        return pairs
            .OrderBy(x => x.Exercice, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.LigneBudgetaire, StringComparer.OrdinalIgnoreCase)
            .Select(x => (x.Exercice, x.LigneBudgetaire))
            .ToList();
    }

    // ========================================================================
    // 5) Helpers de mise en forme
    // ========================================================================

    /// <summary>Construit une feuille "simple" (en-tête coloré + lignes vides éditables).</summary>
    private static void BuildSimpleSheet(IXLWorksheet ws, string[] columns)
    {
        WriteHeaderRow(ws, columns);
        AutoWidth(ws, columns);

        // Lignes vides éditables, prêtes à recevoir les listes déroulantes
        // (équivalent de : for (i<GENERAL_DATA_ROWS) ws.addRow(new Array(columns.length).fill('')) )
        for (int r = 0; r < DataRows; r++)
            for (int c = 1; c <= columns.Length; c++)
                ws.Cell(2 + r, c).SetValue(string.Empty);
    }

    /// <summary>
    /// Écrit la ligne d'en-tête avec la même logique de couleur que headerFillFor() en JS :
    /// rouge = obligatoire, jaune = obligatoire conditionnel, vert = optionnel.
    /// </summary>
    private static void WriteHeaderRow(IXLWorksheet ws, string[] columns)
    {
        var row = ws.Row(1);
        for (int i = 0; i < columns.Length; i++)
        {
            var cell = row.Cell(i + 1);
            cell.Value = columns[i];

            var (fill, font) = HeaderColorFor(columns[i]);
            cell.Style.Fill.BackgroundColor = fill;
            cell.Style.Font.FontColor = font;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }
        row.Height = 32;
    }

    private static void StyleHeaderNoColor(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Alignment.WrapText = true;
    }

    /// <summary>Équivalent de headerFillFor() côté JS.</summary>
    private static (XLColor Fill, XLColor Font) HeaderColorFor(string label)
    {
        var t = label.ToLowerInvariant();
        if (t.Contains("obligatoire:oui si"))
            return (XLColor.FromArgb(0xFF, 0xD5, 0x4F), XLColor.Black);   // jaune, texte noir
        if (t.Contains("obligatoire:oui"))
            return (XLColor.FromArgb(0xE5, 0x39, 0x35), XLColor.White);   // rouge, texte blanc
        return (XLColor.FromArgb(0x43, 0xA0, 0x47), XLColor.White);       // vert, texte blanc
    }

    /// <summary>Équivalent de applyColumnWidths() côté JS.</summary>
    private static void AutoWidth(IXLWorksheet ws, string[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            var width = Math.Min(42, Math.Max(16, (int)Math.Ceiling(columns[i].Length * 0.9)));
            ws.Column(i + 1).Width = width;
        }
    }

    /// <summary>Équivalent de columnLetter() côté JS : convertit un index de colonne (1=A, 2=B, ...) en lettre Excel.</summary>
    private static string ColumnLetter(int colNumber)
    {
        var letter = string.Empty;
        while (colNumber > 0)
        {
            var rem = (colNumber - 1) % 26;
            letter = (char)('A' + rem) + letter;
            colNumber = (colNumber - 1) / 26;
        }
        return letter;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}