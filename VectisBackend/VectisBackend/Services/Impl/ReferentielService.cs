using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VectisBackend.Data;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Services;

public class ReferentielService : IReferentielService
{
    private readonly VectisDbContext _db;

    public ReferentielService(VectisDbContext db)
    {
        _db = db;
    }

    //Referentiel Fiche Generale
    public async Task<FicheGeneraleCodes> ResolveFicheGeneraleCodesAsync(
        FicheGeneraleRowDto row, CancellationToken ct, List<ImportErrorDto> errors)
    {
        decimal? codeTypeContract = null;
        decimal? codeTypeBudget = null;
        int? codeTypePrestation = null;
        decimal? codeNaturePrestation = null;
        decimal? codeCentreGestionnaire = null;
        decimal? codeCentreBeneficiaire = null;
        decimal? codeMaitreOuvrage = null;
        decimal? codeModePaiement = null;
        decimal? codeBanquePaiement = null;
        decimal? codePersonneHabilitee = null;
        decimal? codeAgentChargeSuivi = null;
        decimal? delaiExecution = null;
        decimal? delaiGarantie = null;
        decimal? uniteDelaiExecution = null;
        decimal? uniteDelaiGarantie = null;

        //Type depense (obligatoire)
        codeTypeContract = await _db.DepTypeContrats
            .Where(x => x.LibelleTypeContrat != null && x.LibelleTypeContrat == row.TypeContract)
            .Select(x => (decimal?)x.CodeTypeContrat)
            .FirstOrDefaultAsync(ct);

        if (!codeTypeContract.HasValue)
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Type Contract", $"Introuvable: {row.TypeContract}");

        // Exercice (obligatoire)
        if (string.IsNullOrWhiteSpace(row.Exercice))
        {
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Exercice", "L'exercice est obligatoire");
        }
        else
        {
            // Convertir en decimal pour correspondre au type de la base de données
            if (!decimal.TryParse(row.Exercice.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var annee))
            {
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Exercice", $"Valeur invalide: '{row.Exercice}'");
            }
            else
            {
                var exists = await _db.BdgExercices.AnyAsync(
                    x => x.Cloture == 0 && x.Ouvert == 1 && x.Annee == annee, ct);

                if (!exists)
                {
                    // Debug: vérifier pourquoi ça ne trouve pas
                    var exerciceExiste = await _db.BdgExercices.AnyAsync(x => x.Annee == annee, ct);
                    var exerciceOuvert = await _db.BdgExercices.AnyAsync(x => x.Annee == annee && x.Ouvert == 1, ct);
                    var exerciceNonCloture = await _db.BdgExercices.AnyAsync(x => x.Annee == annee && x.Cloture == 0, ct);

                    string raison = "";
                    if (!exerciceExiste)
                        raison = "n'existe pas dans la base";
                    else if (!exerciceOuvert && !exerciceNonCloture)
                        raison = "est fermé et clôturé";
                    else if (!exerciceOuvert)
                        raison = "est fermé";
                    else if (!exerciceNonCloture)
                        raison = "est clôturé";

                    AddErr(errors, "Fiche Générale", row.ExcelRow, "Exercice", $"Exercice {row.Exercice} {raison}");
                }
            }
        }

        // Fournisseur (obligatoire)
        var frs = await _db.GenFournisseurs
            .Where(x => x.LibelleFrs != null && x.LibelleFrs == row.Fournisseur)
            .Select(x => (decimal?)x.CodeFrs)
            .FirstOrDefaultAsync(ct);

        if (!frs.HasValue)
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Fournisseur", $"Introuvable: {row.Fournisseur}");

        // Devise (obligatoire)
        var dev = await _db.GenDevises
            .Where(x => x.Devise != null && x.Devise == row.Devise)
            .Select(x => (decimal?)x.CodeDevise)
            .FirstOrDefaultAsync(ct);

        if (!dev.HasValue)
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Devise", $"Introuvable: {row.Devise}");

        // Fonction helper pour extraire proprement "Désignation" et "CentreId"
        (string? Designation, string CentreId) ExtraireDesignationEtCentreId(string? valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur))
                return (null, "");

            var v = valeur.Trim();

            // CentreId vide : la valeur se termine par un tiret (avec ou sans espace après).
            if (v.EndsWith(" -", StringComparison.Ordinal) || v.EndsWith(" - ", StringComparison.Ordinal))
            {
                var designationSansTiret = v.TrimEnd().TrimEnd('-').TrimEnd();
                return (designationSansTiret, "");
            }

            var idx = v.LastIndexOf(" - ", StringComparison.Ordinal);
            if (idx < 0)
                return (v, ""); // pas de séparateur trouvé : toute la valeur est la désignation

            var designation = v[..idx].Trim();
            var centreId = v[(idx + 3)..].Trim();
            return (designation, centreId);
        }

        // Centre gestionnaire (obligatoire)
        var (designationGestionnaire, centreIdGestionnaire) = ExtraireDesignationEtCentreId(row.CentreGestionnaire);

        if (string.IsNullOrEmpty(designationGestionnaire))
        {
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Centre Gestionnaire", "Centre gestionnaire obligatoire");
        }
        else
        {
            codeCentreGestionnaire = await ResolveCentreCodeAsync(designationGestionnaire, centreIdGestionnaire, ct);

            if (!codeCentreGestionnaire.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Centre Gestionnaire", $"Introuvable: '{row.CentreGestionnaire}' (recherché: '{designationGestionnaire}' / CentreId '{centreIdGestionnaire}')");
        }

        // Centre bénéficiaire (obligatoire)
        var (designationBeneficiaire, centreIdBeneficiaire) = ExtraireDesignationEtCentreId(row.CentreBeneficiaire);

        if (string.IsNullOrEmpty(designationBeneficiaire))
        {
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Centre bénéficiaire", "Centre bénéficiaire obligatoire");
        }
        else
        {
            codeCentreBeneficiaire = await ResolveCentreCodeAsync(designationBeneficiaire, centreIdBeneficiaire, ct);

            if (!codeCentreBeneficiaire.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Centre bénéficiaire", $"Introuvable: '{row.CentreBeneficiaire}' (recherché: '{designationBeneficiaire}' / CentreId '{centreIdBeneficiaire}')");
        }

        // Type budget (optionnel)
        if (!string.IsNullOrWhiteSpace(row.TypeBudget))
        {
            codeTypeBudget = await _db.BdgTypeBudgets
                .Where(x => x.DesignationTypeBdg != null && x.DesignationTypeBdg == row.TypeBudget)
                .Select(x => (decimal?)x.CodeTypeBdg)
                .FirstOrDefaultAsync(ct);

            if (!codeTypeBudget.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Type budget", $"Introuvable: {row.TypeBudget}");
        }

        // Type prestation (optionnel)
        if (!string.IsNullOrWhiteSpace(row.TypePrestation))
        {
            codeTypePrestation = await _db.DepTypePrestations
                .Where(x => x.Designation != null && x.Designation == row.TypePrestation)
                .Select(x => (int?)x.CodeTypePrestation)
                .FirstOrDefaultAsync(ct);

            if (!codeTypePrestation.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Type prestation", $"Introuvable: {row.TypePrestation}");
        }

        // Nature prestation (obligatoire)
        var natureQuery = _db.DepNaturePrestations
            .Where(x => x.LibelleNature != null && x.LibelleNature == row.NaturePrestation);

        if (codeTypeContract.HasValue)
            natureQuery = natureQuery.Where(x => x.CodeTypeDepense == codeTypeContract.Value);

        if (codeTypePrestation.HasValue)
            natureQuery = natureQuery.Where(x => x.CodeTypePrestation == codeTypePrestation.Value);

        codeNaturePrestation = await natureQuery
            .Select(x => (decimal?)x.CodeNature)
            .FirstOrDefaultAsync(ct);

        if (!codeNaturePrestation.HasValue)
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Nature prestation", $"Introuvable: {row.NaturePrestation}");

        // Maître d'ouvrage (optionnel, format possible "NOM PRENOM")
        if (!string.IsNullOrWhiteSpace(row.MaitreOuvrage))
        {
            var mo = await _db.BdgMaitreOuvrages
                .Where(x =>
                    ((x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim() == row.MaitreOuvrage ||
                    (x.Nom ?? "") == row.MaitreOuvrage)
                .Select(x => (decimal?)x.CodeMaitreOuvrage)
                .FirstOrDefaultAsync(ct);

            codeMaitreOuvrage = mo;
            if (!codeMaitreOuvrage.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Maître d'ouvrage", $"Introuvable: {row.MaitreOuvrage}");
        }

        // Mode paiement (obligatoire)
        codeModePaiement = await _db.GenTypeReglements
            .Where(x => x.LibelleTypeReglement != null && x.LibelleTypeReglement == row.ModePaiement)
            .Select(x => (decimal?)x.CodeTypeReglement)
            .FirstOrDefaultAsync(ct);

        if (!codeModePaiement.HasValue)
            AddErr(errors, "Fiche Générale", row.ExcelRow, "Mode de paiement", $"Introuvable: {row.ModePaiement}");

        // Banque (conditionnel)
        var isVirement = row.ModePaiement?.Trim().Equals("Virement", StringComparison.OrdinalIgnoreCase) == true;
        if (isVirement)
        {
            var comptesFournisseur = frs.HasValue
                ? await GetComptesBancairesFournisseurAsync(frs.Value, ct)
                : new List<CompteBancaireInfo>();

            if (string.IsNullOrWhiteSpace(row.Banque))
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Banque", "Obligatoire si virement.");
            else
            {
                var matchBanque = comptesFournisseur.FirstOrDefault(c => c.BanqueLabel == row.Banque);
                if (matchBanque != null)
                    codeBanquePaiement = matchBanque.CodeCptBancaire;
                else
                    AddErr(errors, "Fiche Générale", row.ExcelRow, "Banque", $"Introuvable pour ce fournisseur: {row.Banque}");
            }

            if (string.IsNullOrWhiteSpace(row.NumeroCompte))
                AddErr(errors, "Fiche Générale", row.ExcelRow, "N° compte", "Obligatoire si virement.");
            else
            {
                var matchCompte = comptesFournisseur.FirstOrDefault(c =>
                    c.NumCompte == row.NumeroCompte
                    && (!codeBanquePaiement.HasValue || c.CodeCptBancaire == codeBanquePaiement.Value));

                if (matchCompte == null)
                    AddErr(errors, "Fiche Générale", row.ExcelRow, "N° compte", $"Introuvable: {row.NumeroCompte}");
            }
        }

        // Personne habilitée (optionnel/conditionnel)
        if (!string.IsNullOrWhiteSpace(row.PersonneHabilitee))
        {
            codePersonneHabilitee = await _db.RefAgents
                .Where(x => ((x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim() == row.PersonneHabilitee && x.PersonneHabilitee == 1)
                .Select(x => (decimal?)x.CodeAgent)
                .FirstOrDefaultAsync(ct);

            if (!codePersonneHabilitee.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Personne Habilitée", $"Introuvable: {row.PersonneHabilitee}");
        }

        // Chargé de suivi (optionnel, format "MATRICULE - NOM PRENOM")
        if (!string.IsNullOrWhiteSpace(row.ChargeSuiviAgent))
        {
            var raw = row.ChargeSuiviAgent.Trim();
            var codeAgent = await _db.RefAgents
                .Where(x =>
                    ((x.Matricule ?? "") + " - " + (x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim() == raw
                    || ((x.Nom ?? "") + " " + (x.Prenom ?? "")).Trim() == raw
                    || (x.Matricule ?? "") == raw)
                .Select(x => (decimal?)x.CodeAgent)
                .FirstOrDefaultAsync(ct);

            codeAgentChargeSuivi = codeAgent;
            if (!codeAgentChargeSuivi.HasValue)
                AddErr(errors, "Fiche Générale", row.ExcelRow, "Chargé de suivi", $"Introuvable: {row.ChargeSuiviAgent}");
        }

        // Délai exécution et garantie
        var delaiEx = row.DelaiExecution?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var delaiGar = row.DelaiGarantie?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        decimal.TryParse(delaiEx?[0].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var nombreEx);
        var periodeEx = delaiEx?[1].Trim();
        decimal.TryParse(delaiGar?[0].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var nombreGar);
        var periodeGar = delaiGar?[1].Trim();

        delaiExecution = nombreEx;
        delaiGarantie = nombreGar;

        // Unités délai 
        uniteDelaiExecution = await _db.DepPeriodes
            .Where(x => x.LibellePeriode != null && x.LibellePeriode == periodeEx)
            .Select(x => (decimal?)x.CodePeriode)
            .FirstOrDefaultAsync(ct);

        uniteDelaiGarantie = await _db.DepPeriodes
            .Where(x => x.LibellePeriode != null && x.LibellePeriode == periodeGar)
            .Select(x => (decimal?)x.CodePeriode)
            .FirstOrDefaultAsync(ct);

        return new FicheGeneraleCodes(
            CodeTypeContract: codeTypeContract ?? 0,
            CodeFournisseur: frs ?? 0,
            CodeDevise: dev ?? 0,
            CodeCentreBeneficiaire: codeCentreBeneficiaire ?? 0,
            CodeTypeBudget: codeTypeBudget ?? 0,
            CodeTypePrestation: codeTypePrestation,
            CodeNaturePrestation: codeNaturePrestation,
            CodeCentreGestionnaire: codeCentreGestionnaire,
            CodeMaitreOuvrage: codeMaitreOuvrage,
            CodeModePaiement: codeModePaiement,
            CodeBanquePaiement: codeBanquePaiement,
            CodePersonneHabilitee: codePersonneHabilitee,
            CodeAgentChargeSuivi: codeAgentChargeSuivi,
            DelaiExecution: delaiExecution,
            DelaiGarantie: delaiGarantie,
            UniteDelaiExecution: uniteDelaiExecution,
            UniteDelaiGarantie: uniteDelaiGarantie
        );
    }

    //Referentiel Bordereau
    public async Task<(decimal codeProduit, string produitId, string designation, string? unite)?> ResolveProduitCodeAsync(string designationProduit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(designationProduit))
            return null;

        var designationNettoyee = designationProduit.Trim();

        var query = from brd in _db.GenProduitBrds
                    join prd in _db.GenProduits
                        on brd.CodeProduit equals prd.CodeProduit
                    where brd.PrestationArchitecte == 0
                        && prd.ProduitGenerique != 1
                        && (prd.DateFermeture == null ||
                            prd.DateFermeture >= DateTime.Today)
                        && brd.Designation != null
                        && (prd.ProduitId + " | " + prd.Designation) == designationNettoyee
                    select new
                    {
                        prd.CodeProduit,
                        prd.ProduitId,
                        prd.Designation,
                        prd.CodeUnite
                    };

        var prod = await query.FirstOrDefaultAsync(ct);

        if (prod == null)
            return null;

        return (
            prod.CodeProduit,
            prod.ProduitId ?? "",
            prod.Designation ?? "",
            prod.CodeUnite.HasValue ? prod.CodeUnite.Value.ToString() : ""
        );
    }

    //Referentiel Engagement
    public async Task<EngagementCodes> ResolveEngagementCodesAsync(EngagementRowDto row, CancellationToken ct, List<ImportErrorDto> errors)
    {
        decimal? codeTypeCredit = null;
        decimal? annee = null;
        decimal? codeTypeContract = null;
        decimal? codeDepense = null;
        decimal? codeLigne = null;
        decimal? disponibleCP = null;
        double? disponibleCE = null;
        double? creditPreEngCP = null;
        double? creditPreEngCE = null;
        double? creditEngCP = null;
        double? creditEngCE = null;
        double? creditDef = null;

        // Type crédit (obligatoire)
        codeTypeCredit = await _db.CptTypeCreditSupps
            .Where(x => x.DesignationTypeCredit != null && x.DesignationTypeCredit == row.CategorieBudget)
            .Select(x => (decimal?)x.CodeTypeCredit)
            .FirstOrDefaultAsync(ct);

        if (!codeTypeCredit.HasValue && !string.IsNullOrWhiteSpace(row.CategorieBudget))
            AddErr(errors, "Engagement", row.ExcelRow, "Catégorie budget", $"Introuvable: {row.CategorieBudget}");

        // N Depense (obligatoire)
        codeDepense = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => (decimal?)x.CodeMarche)
            .FirstOrDefaultAsync(ct);

        if (!codeDepense.HasValue)
            AddErr(errors, "Engagement", row.ExcelRow, "N° dépense", $"Introuvable: {row.NumeroDepense}");

        // Annee
        annee = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => Math.Truncate(Convert.ToDecimal(x.Exercice)))
            .FirstOrDefaultAsync(ct);

        if (!annee.HasValue)
            AddErr(errors, "Engagement", row.ExcelRow, "Exercice", $"Introuvable pour la dépense: {row.NumeroDepense}");

        // Type depense
        codeTypeContract = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => (decimal?)x.CodeTypeContrat)
            .FirstOrDefaultAsync(ct);

        if (!codeTypeContract.HasValue)
            AddErr(errors, "Engagement", row.ExcelRow, "Type dépense", $"Introuvable pour la dépense: {row.NumeroDepense}");

        // Code ligne budgetaire
        var anneeString = annee.HasValue ? ((int)annee.Value).ToString() : string.Empty;
        var codeLigneBdg = row.CodeLigneBudgetaire?.Trim();

        codeLigne = await _db.BdgStructureBudgetaireGenHs
            .Where(bdg => ((bdg.Codification ?? "") + " - " + (bdg.Libelle ?? "")).Trim() == codeLigneBdg
                && bdg.Annee == anneeString
                && bdg.CategorieBudget != 1
                && bdg.TypeCategorieBudget == 0
                && bdg.TypeBudget == 1
                && bdg.ActiveLigne == 1
                && !string.IsNullOrEmpty(bdg.Codification)
                && _db.BdgStructureBudgetaireGenHs
                    .Where(vbh => vbh.CodeLigneBdg == bdg.CodeLigneBdg
                        && (vbh.Bop != 1 || vbh.Bop == null))
                    .Any()
            )
            .Select(bdg => bdg.CodeLigneBdg)
            .FirstOrDefaultAsync(ct);

        if (!codeLigne.HasValue)
            AddErr(errors, "Engagement", row.ExcelRow, "Code ligne budgétaire", $"Introuvable pour l'année: {anneeString}");

        //

        // Credit disponible CP ; Credit disponible CE ; Credit pré-engagé CP ; Credit pré-engagé CE
        // Credit engagé CP ;Credit engagé CE
        var resultat = await (
            from bdgd in _db.BdgDotationCreditSupps
            join bdgl in _db.BdgDotationLigneBudgetaires
                on bdgd.CodeLigne equals bdgl.CodeLigne into bdglGroup
            from bdgl in bdglGroup.DefaultIfEmpty() // LEFT JOIN
            where bdgd.CodeTypeCredit == codeTypeCredit
                && bdgd.CodeLigne == codeLigne
                && bdgd.Annee == anneeString
            select new
            {
                CreditDisponibleCP = bdgl != null ? (bdgl.CreditDisponibleCp ?? 0) : 0,
                CreditDisponibleCE = bdgl != null ? (bdgl.CreditDisponibleCe ?? 0) : 0,
                CreditEngageCP = bdgl != null ? (bdgl.CreditEngageCp ?? 0) : 0,
                CreditEngageCE = bdgl != null ? (bdgl.CreditEngageCe ?? 0) : 0,
                CreditPreEngCP = bdgl != null ? (bdgl.CreditPreEngageCp ?? 0) : 0,
                CreditPreEngCE = bdgl != null ? (bdgl.CreditPreEngageCe ?? 0) : 0,
                CreditDefinitif = bdgl != null ? (bdgl.CreditDefinitif ?? 0) : 0
            }
        ).FirstOrDefaultAsync(ct);

        if (resultat != null)
        {
            disponibleCP = resultat.CreditDisponibleCP;
            disponibleCE = resultat.CreditDisponibleCE;
            creditPreEngCP = resultat.CreditPreEngCP;
            creditPreEngCE = resultat.CreditPreEngCE;
            creditEngCP = resultat.CreditEngageCP;
            creditEngCE = resultat.CreditEngageCE;
            creditDef = resultat.CreditDefinitif;
        }

        return new EngagementCodes(
            CodeTypeCredit: codeTypeCredit ?? 0,
            Annee: annee ?? 0,
            CodeTypeContract: codeTypeContract ?? 0,
            CodeDepense: codeDepense ?? 0,
            CodeLigne: codeLigne ?? 0,
            DisponibleCP: disponibleCP ?? 0,
            DisponibleCE: disponibleCE ?? 0,
            CreditPreEngCP: creditPreEngCP ?? 0,
            CreditPreEngCE: creditPreEngCE ?? 0,
            CreditEngCP: creditEngCP ?? 0,
            CreditEngCE: creditEngCE ?? 0,
            CreditDef: creditDef ?? 0
        );
    }

    // Referentiel Livraison
    public async Task<LivraisonCodes> ResolveLivraisonCodesAsync(LivraisonRowDto row, CancellationToken ct, List<ImportErrorDto> errors)
    {
        decimal? CodeDepense = null;
        decimal? CodeResponsable = null;
        decimal? CodeMagasin = null;
        decimal? CodeMagasinier = null;
        string? CodeFilsProduit = null;
        decimal? TauxTva = null;
        decimal? Quantite = null;
        decimal? CodeCentreResp = null;

        // Code dépense (obligatoire)
        var depenseInfo = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => new { x.CodeMarche, x.CodeCentreGes })
            .FirstOrDefaultAsync(ct);
        CodeDepense = depenseInfo?.CodeMarche;

        if (!CodeDepense.HasValue)
            AddErr(errors, "Livraison", row.ExcelRow, "N° dépense", $"Introuvable: {row.NumeroDepense}");

        // Code responsable (obligatoire)
        CodeResponsable = await (
            from rf in _db.RefAgents
            join cr in _db.GenCentreRespContacts on rf.CodeAgent equals cr.CodeContact
            where ((rf.Nom ?? "") + " " + (rf.Prenom ?? "")).Trim() == row.ResponsableLivraison
            select (decimal?)cr.CodeContact
            ).FirstOrDefaultAsync(ct);

        if (!CodeResponsable.HasValue && !string.IsNullOrWhiteSpace(row.ResponsableLivraison))
            AddErr(errors, "Livraison", row.ExcelRow, "Responsable livraison", $"Introuvable: {row.ResponsableLivraison}");

        // Code magasin (obligatoire)
        var magasinierCodes = await _db.StkMagasiniers
            .Where(sm => sm.Id == 1)
            .Select(sm => sm.CodeMagasinier)
            .ToListAsync(ct);

        CodeMagasin = await (
            from gm in _db.GenMagasins
            join smm in _db.StkMagasinMagasiniers on gm.CodeMagasin equals (decimal?)smm.CodeMagasin
            join gv in _db.GenVilles on gm.CodeVille equals gv.CodeVille into villeGroup
            from gv in villeGroup.DefaultIfEmpty()
            where gm.MagasinReforme != 1
                && magasinierCodes.Contains((decimal)smm.CodeMagasinier)
                && ((gm.MagasinId ?? "") + " " + (gm.Libelle ?? "") + " " + (gv.DescriptionVille ?? "")).Trim() == row.Magasin
            select (decimal?)gm.CodeMagasin
            ).FirstOrDefaultAsync(ct);

        if (!CodeMagasin.HasValue && !string.IsNullOrWhiteSpace(row.Magasin))
            AddErr(errors, "Livraison", row.ExcelRow, "Magasin", $"Introuvable pour le magasinier: {row.Magasinier}");

        // Code magasinier (obligatoire)
        CodeMagasinier = await (
            from sm in _db.StkMagasiniers
            join smm in _db.StkMagasinMagasiniers on sm.CodeMagasinier equals (decimal?)smm.CodeMagasinier
            where smm.CodeMagasin == CodeMagasin && ((sm.Nom ?? "") + " " + (sm.Prenom ?? "")).Trim() == row.Magasinier
            select (decimal?)sm.CodeMagasinier
            ).FirstOrDefaultAsync(ct);

        if (!CodeMagasinier.HasValue && !string.IsNullOrWhiteSpace(row.Magasinier))
            AddErr(errors, "Livraison", row.ExcelRow, "Magasinier", $"Introuvable pour le magasin: {row.Magasin}");

        // Code fils produit (obligatoire)
        var produitInfos = await (
            from dmhp in _db.DepMarchesH
            join dmh in _db.DepMarchesH on dmhp.CodePere equals dmh.CodeFils
            where dmh.Libelle == row.NumeroDepense && dmhp.Libelle == row.ProduitNPrix
            select new { dmhp.CodeFils, dmhp.Quantite, dmhp.TauxTva }
            ).FirstOrDefaultAsync(ct);

        CodeFilsProduit = produitInfos?.CodeFils;

        if (string.IsNullOrWhiteSpace(CodeFilsProduit))
            AddErr(errors, "Livraison", row.ExcelRow, "Produit N° prix", $"Introuvable: {row.ProduitNPrix}");

        // Taux TVA (obligatoire)
        TauxTva = produitInfos?.TauxTva;

        if (!TauxTva.HasValue)
            AddErr(errors, "Livraison", row.ExcelRow, "Taux TVA", $"Introuvable pour le produit: {row.ProduitNPrix}");

        // Quantité (obligatoire)
        Quantite = produitInfos?.Quantite;

        if (!Quantite.HasValue)
            AddErr(errors, "Livraison", row.ExcelRow, "Quantité", $"Introuvable pour le produit: {row.ProduitNPrix}");

        // Code centre responsable (obligatoire)
        CodeCentreResp = depenseInfo?.CodeCentreGes;

        if (!CodeCentreResp.HasValue)
            AddErr(errors, "Livraison", row.ExcelRow, "Centre responsable", $"Introuvable pour la dépense: {row.NumeroDepense}");

        return new LivraisonCodes(
            CodeDepense: CodeDepense ?? 0,
            CodeResponsable: CodeResponsable,
            CodeMagasin: CodeMagasin,
            CodeMagasinier: CodeMagasinier,
            CodeFilsProduit: CodeFilsProduit ?? "",
            TauxTva: TauxTva ?? 0,
            Quantite: Quantite ?? 0,
            CodeCentreResp: CodeCentreResp ?? 0
        );
    }

    // Referentiel Reception
    public async Task<ReceptionCodes> ResolveReceptionCodesAsync(ReceptionRowDto row, CancellationToken ct, List<ImportErrorDto> errors)
    {
        decimal CodeDepense = 0;
        decimal? CodeNature = null;
        decimal? CodeType = null;
        decimal? CodeReleve = null;

        // Code dépense (obligatoire)
        CodeDepense = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => (decimal)x.CodeMarche)
            .FirstOrDefaultAsync(ct);

        if (CodeDepense == 0)
            AddErr(errors, "Réception", row.ExcelRow, "N° dépense", $"Introuvable: {row.NumeroDepense}");

        // Code nature (obligatoire)
        CodeNature = await _db.DepNatureReceptions
            .Where(x => x.LibelleNatureReception != null && x.LibelleNatureReception == row.NatureReception && x.CodeNatureReception != 3)
            .Select(x => (decimal?)x.CodeNatureReception)
            .FirstOrDefaultAsync(ct);

        if (!CodeNature.HasValue && !string.IsNullOrWhiteSpace(row.NatureReception))
            AddErr(errors, "Réception", row.ExcelRow, "Nature réception", $"Introuvable: {row.NatureReception}");

        // Code type (obligatoire)
        CodeType = await _db.DepTypeReceptions
            .Where(x => x.LibelleTypeReception != null && x.LibelleTypeReception == row.TypeReception)
            .Select(x => (decimal?)x.CodeTypeReception)
            .FirstOrDefaultAsync(ct);

        if (!CodeType.HasValue && !string.IsNullOrWhiteSpace(row.TypeReception))
            AddErr(errors, "Réception", row.ExcelRow, "Type réception", $"Introuvable: {row.TypeReception}");

        // Code relevé (obligatoire)
        CodeReleve = await _db.DepReleveMarches
            .Where(x => x.ReferenceLivraison != null && x.ReferenceLivraison == row.NumeroLivraison && x.CodeMarche == CodeDepense)
            .Select(x => (decimal?)x.CodeReleve)
            .FirstOrDefaultAsync(ct);

        if (!CodeReleve.HasValue)
            AddErr(errors, "Réception", row.ExcelRow, "Relevé réception", $"Introuvable: {row.NumeroLivraison}");

        return new ReceptionCodes(
            CodeDepense: CodeDepense,
            CodeNature: CodeNature,
            CodeType: CodeType,
            CodeReleve: CodeReleve
        );
    }

    // Referntiel Facture
    public async Task<FactureCodes> ResolveFactureCodesAsync(FactureRowDto row, CancellationToken ct, List<ImportErrorDto> errors)
    {
        decimal? CodeDepense = null;
        decimal? CodeReleve = null;
        decimal? Periode = null;
        decimal? CodeFournisseur = null;
        decimal? MontantFactureHT = null;
        decimal? MontantFactureTTC = null;
        List<ProduitFactureItem>? Produits = null;
        decimal? TauxTvaFacture = null;
        decimal? TauxRemiseFacture = null;
        decimal? TauxRsFacture = null;

        // Code depense
        var depenseInfos = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => new { x.CodeMarche, x.CodeFournisseur })
            .FirstOrDefaultAsync(ct);
        CodeDepense = depenseInfos?.CodeMarche;

        if (!CodeDepense.HasValue)
            AddErr(errors, "Facture", row.ExcelRow, "N° Dépense", $"Introuvable : {row.NumeroDepense}");

        // Code Releve
        var factureInfos = await _db.DepReleveMarches
            .Where(x => x.ReferenceLivraison == row.NumeroLivraison && x.CodeMarche == CodeDepense)
            .Select(x => new { x.CodeReleve, x.MtReleveHt, x.MtReleveTtc })
            .FirstOrDefaultAsync(ct);

        if (factureInfos == null)
        {
            AddErr(errors, "Facture", row.ExcelRow, "N° Livraison", $"Introuvable: {row.NumeroLivraison}");
        }

        CodeReleve = factureInfos?.CodeReleve;

        if (!CodeReleve.HasValue)
            AddErr(errors, "Facture", row.ExcelRow, "N° Livraison", $"Introuvable: {row.NumeroLivraison}");

        // Periode
        string[] annee =
        {
            "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
            "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"
        };

        if (!string.IsNullOrWhiteSpace(row.Periode))
        {
            int index = Array.FindIndex(annee, m => m.Equals(row.Periode.Trim(), StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                Periode = index + 1; // +1 car les tableaux C# commencent à index 0
            }
            else AddErr(errors, "Facture", row.ExcelRow, "Période", $"La période '{row.Periode}' n'est pas un nom de mois valide.");
        }

        // Fournisseur
        CodeFournisseur = depenseInfos?.CodeFournisseur;

        if (!CodeFournisseur.HasValue)
            AddErr(errors, "Facture", row.ExcelRow, "Fournisseur", $"Introuvable : {row.NumeroDepense}");

        // MontantHT Facture
        MontantFactureHT = factureInfos?.MtReleveHt;

        if (!MontantFactureHT.HasValue)
            AddErr(errors, "Facture", row.ExcelRow, "Montant HT", $"Introuvable : {row.NumeroLivraison}");

        // MontantTTC Facture
        MontantFactureTTC = factureInfos?.MtReleveTtc;

        if (!MontantFactureTTC.HasValue)
            AddErr(errors, "Facture", row.ExcelRow, "Montant TTC", $"Introuvable : {row.NumeroLivraison}");

        // Produits livraison
        Produits = await (
            from dlr in _db.DepLigneReleveMarches
            where factureInfos != null && dlr.CodeReleve == factureInfos.CodeReleve
            join dvr in _db.DepVentilQuantiteReleves
                on new
                {
                    CodeReleve = (decimal?)dlr.CodeReleve,
                    CodeFilsProduit = (string?)dlr.CodeFilsProduit
                }
                equals new
                {
                    CodeReleve = (decimal?)dvr.CodeReleve,
                    CodeFilsProduit = (string?)dvr.CodeFilsProduit
                }
            join dmh in _db.DepMarchesH on dlr.CodeFilsProduit equals dmh.CodeFils
            select new ProduitFactureItem(
                dmh.ReferenceProduit,
                (string?)dlr.CodeFilsProduit,
                dlr.QteLiv,
                dvr.QuantiteAcpt,
                dmh.PrixUnit,
                dlr.TauxTva,
                dmh.Remise,
                dvr.CodeCentre
            )
            ).ToListAsync(ct);

        // TVA Facture
        var tauxInfos = await _db.DepMarches
            .Where(x => x.NumeroMarche != null && x.NumeroMarche == row.NumeroDepense)
            .Select(x => new { x.TauxTva, x.TauxRabais, x.TauxRetenueSource })
            .FirstOrDefaultAsync(ct);

        if (tauxInfos == null)
        {
            AddErr(errors, "Facture", row.ExcelRow, "Marché", $"Marché introuvable : {row.NumeroDepense}");
        }
        else
        {
            // Conversion sécurisée en decimal?
            TauxTvaFacture = tauxInfos.TauxTva.HasValue ? Convert.ToDecimal(tauxInfos.TauxTva) : null;
            TauxRemiseFacture = tauxInfos.TauxRabais.HasValue ? Convert.ToDecimal(tauxInfos.TauxRabais) : null;
            TauxRsFacture = tauxInfos.TauxRetenueSource.HasValue ? Convert.ToDecimal(tauxInfos.TauxRetenueSource) : null;

            // Validation TVA
            if (!TauxTvaFacture.HasValue)
                AddErr(errors, "Facture", row.ExcelRow, "Taux TVA", $"Taux TVA non renseigné pour le marché : {row.NumeroDepense}");

            // Validation Remise
            if (!TauxRemiseFacture.HasValue)
                AddErr(errors, "Facture", row.ExcelRow, "Taux Remise", $"Taux Remise non renseigné pour le marché : {row.NumeroDepense}");

            // Validation RS (CORRIGÉ : on vérifie TauxRsFacture ici)
            if (!TauxRsFacture.HasValue)
                AddErr(errors, "Facture", row.ExcelRow, "Taux RS", $"Taux RS non renseigné pour le marché : {row.NumeroDepense}");
        }

        return new FactureCodes(
            CodeDepense: CodeDepense ?? 0,
            CodeReleve: CodeReleve ?? 0,
            Periode: Periode,
            CodeFournisseur: CodeFournisseur ?? 0,
            MontantFactureHT: MontantFactureHT ?? 0,
            MontantFactureTTC: MontantFactureTTC ?? 0,
            Produits: Produits,
            TauxTvaFacture: TauxTvaFacture ?? 0m,
            TauxRemiseFacture: TauxRemiseFacture ?? 0m,
            TauxRsFacture: TauxRsFacture ?? 0m
        );
    }

    // Referentiel Validation
    public async Task<(int idEtape, int idProcessus)> ResolveValidationCodesAsync(string type, CancellationToken ct, List<ImportErrorDto> errors)
    {
        var etapeValidaion = await _db.EtapeValidations
            .Where(x => x.Designation != null && x.Designation == "Validation " + type && x.Niveau == 1)
            .Select(x => new { x.Id, x.IdProcessus })
            .FirstOrDefaultAsync(ct);

        if (etapeValidaion == null)
            AddErr(errors, "Validation", 0, "Type validation", $"Introuvable: Validation {type}");

        return etapeValidaion != null
            ? (idEtape: etapeValidaion.Id, idProcessus: etapeValidaion.IdProcessus)
            : (idEtape: 0, idProcessus: 0);
    }

    /// <summary>
    /// Résout le code d'un centre (gestionnaire ou bénéficiaire) à partir de sa Désignation et
    /// de son CentreId (extraits du libellé exporté "Designation - CentreId"). Reproduit
    /// EXACTEMENT la même jointure que ExportTemplateService.BuildReferentielDataAsync pour
    /// "Centre Gestionnaire(GEN_CENTRE_RESPONSABILITE join GEN_CENTRE_RESPONSABILITE_H)" :
    ///   - cr1.Bloquer = 0
    ///   - existence d'une ligne gen_centre_responsabilite_h (crh.ref = cr1.code_centre AND crh.type <> 'Racine')
    ///   - cr2 = centre dont cr1.id_section = cr2.code_centre (LEFT JOIN), pour le CentreId
    /// TODO : adapter le nom du DbSet/des propriétés (Ref, Type, IdSection, Bloquer, CentreId)
    /// si besoin selon le modèle EF réel — voir le même TODO côté ExportTemplateService.
    /// </summary>
    private async Task<decimal?> ResolveCentreCodeAsync(string designation, string centreId, CancellationToken ct)
    {
        var query =
            from cr1 in _db.GenCentreResponsabilites
            where cr1.Bloquer == 0
                  && cr1.Designation != null
                  && cr1.Designation.Trim() == designation
                  && _db.GenCentreResponsabiliteHs.Any(h => h.Ref == cr1.CodeCentre.ToString() && h.Type != "Racine")
            join cr2 in _db.GenCentreResponsabilites
                on cr1.IdSection equals cr2.CodeCentre.ToString() into cr2Group
            from cr2 in cr2Group.DefaultIfEmpty()
            where (cr2 != null ? (cr2.CentreId ?? "") : "") == centreId
            select (decimal?)cr1.CodeCentre;

        return await query.FirstOrDefaultAsync(ct);
    }

    private sealed record CompteBancaireInfo(decimal? CodeCptBancaire, string NumCompte, string BanqueLabel);

    /// <summary>
    /// Liste des comptes bancaires d'un fournisseur donné, avec le MÊME libellé "Banque"
    /// calculé que celui offert dans la liste déroulante en cascade côté export (voir
    /// ExportTemplateService.BuildFournisseurBanqueCascadeAsync / BuildFournisseurCompteCascadeAsync) :
    ///   Banque = (cb.Agence == "A") ? agence.DesignationBanque : cb.Agence
    /// et CodeBanque = code de la banque SIEGE (celui réellement stocké sur GEN_COMPTEBANCAIRE.CODE_BANQUE),
    /// pas celui de l'agence. Equivalent LINQ de la requête SQL "N° Compte" fournie (siège + agence),
    /// scopée à un seul fournisseur.
    /// </summary>
    private async Task<List<CompteBancaireInfo>> GetComptesBancairesFournisseurAsync(decimal codeFrs, CancellationToken ct)
    {
        var query =
            from cb in _db.GenCompteBancaires
            where cb.CodeFrs == codeFrs
            join siege in _db.GenBanques on cb.CodeBanque equals siege.CodeBanque
            where siege.Siege == 1
            join agence in _db.GenBanques on siege.CodeBanque equals agence.CleSiege
            where agence.DesignationBanque == cb.Agence
                  && (agence.Siege == 0 || agence.Siege == null)
            select new
            {
                cb.CodeCptBancaire,
                NumCompte = cb.NumCompte!,
                BanqueLabel = cb.Agence == "A" ? agence.DesignationBanque! : cb.Agence!
            };

        var raw = await query.Distinct().ToListAsync(ct);

        return raw
            .Select(x => new CompteBancaireInfo(x.CodeCptBancaire, x.NumCompte, x.BanqueLabel))
            .ToList();
    }

    private static void AddErr(List<ImportErrorDto> errors, string sheet, int row, string col, string message)
        => errors.Add(new ImportErrorDto { Sheet = sheet, RowNumber = row, Column = col, Message = message });
}