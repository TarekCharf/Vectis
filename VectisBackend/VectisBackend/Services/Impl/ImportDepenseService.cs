using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VectisBackend.Data;
using VectisBackend.Entities;
using VectisBackend.Models;
using VectisBackend.Models.Import;
using VectisBackend.Parsers;
using VectisBackend.Services.ImplMappers;

namespace VectisBackend.Services.Impl;

public class ImportDepenseService : IImportDepenseService
{
    private readonly VectisDbContext _db;
    private readonly IEnumerable<IExcelSheetParser> _parsers;
    private readonly IFicheGeneraleMapperService _ficheMapper;
    private readonly IBorderauxMapperService _bordMapper;
    private readonly IOrdreServiceMapperService _osMapper;
    private readonly IEngagementMapperService _engMapper;
    private readonly ILivraisonMapperService _livMapper;
    private readonly IReceptionMapperService _recMapper;
    private readonly IFactureMapperService _facMapper;
    private readonly IReferentielService _lookup;

    public ImportDepenseService(
        VectisDbContext db,
        IEnumerable<IExcelSheetParser> parsers,
        IFicheGeneraleMapperService ficheMapper,
        IBorderauxMapperService bordMapper,
        IOrdreServiceMapperService osMapper,
        IEngagementMapperService engMapper,
        ILivraisonMapperService livMapper,
        IReceptionMapperService recMapper,
        IFactureMapperService facMapper,
        IReferentielService lookup)
    {
        _db = db;
        _parsers = parsers;
        _ficheMapper = ficheMapper;
        _bordMapper = bordMapper;
        _osMapper = osMapper;
        _engMapper = engMapper;
        _livMapper = livMapper;
        _recMapper = recMapper;
        _facMapper = facMapper;
        _lookup = lookup;
    }

    // Fonction helper pour mapper les exceptions en DatabaseErrorDto
    private DatabaseErrorDto MapExceptionToDbError(Exception ex)
    {
        var dbError = new DatabaseErrorDto
        {
            Message = "Erreur lors de la sauvegarde en base de données",
            Timestamp = DateTime.UtcNow
        };

        if (ex is DbUpdateException dbEx)
        {
            dbError.ErrorType = "UpdateError";
            dbError.SqlErrorCode = "DB_UPDATE";
            dbError.TechnicalMessage = dbEx.ToString();

            if (dbEx.InnerException is SqlException sqlEx)
            {
                dbError.SqlErrorCode = sqlEx.Number.ToString();
                dbError.TechnicalMessage = sqlEx.Message;

                switch (sqlEx.Number)
                {
                    case 547:
                        dbError.Message = "Violation de contrainte de clé étrangère. Vérifiez que les valeurs référencées existent.";
                        dbError.ErrorType = "ForeignKeyViolation";
                        break;
                    case 2627:
                    case 2601:
                        dbError.Message = "Doublon détecté. Cette valeur existe déjà dans la base de données.";
                        dbError.ErrorType = "UniqueViolation";
                        break;
                    case 515:
                        dbError.Message = "Une valeur obligatoire est manquante.";
                        dbError.ErrorType = "NotNullViolation";
                        break;
                    default:
                        dbError.Message = $"Erreur SQL ({sqlEx.Number}): {sqlEx.Message}";
                        break;
                }
            }
        }
        else
        {
            dbError.ErrorType = "Unexpected";
            dbError.Message = ex.Message;
            dbError.TechnicalMessage = ex.ToString();
        }

        return dbError;
    }

    public async Task<ImportResultDto> ImportAsync(IFormFile file, ImportDepenseRequestDto options, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return Fail("File", 0, "file", "Fichier vide.");

        var errors = new List<ImportErrorDto>();
        var dbErrors = new List<DatabaseErrorDto>();
        var wbData = new WorkbookImportDto();

        using (var stream = file.OpenReadStream())
        using (var wb = new XLWorkbook(stream))
        {
            foreach (var parser in _parsers)
                parser.Parse(wb, options, wbData, errors);
        }

        ValidateAll(wbData, errors);

        if (errors.Any())
            return new ImportResultDto { Success = false, TotalRows = wbData.FicheGenerale.Count, InsertedRows = 0, Errors = errors };
        if (dbErrors.Any())
            return new ImportResultDto { Success = false, TotalRows = wbData.FicheGenerale.Count, InsertedRows = 0, Errors = dbErrors.Select(e => new ImportErrorDto
            {
                Sheet = e.Sheet,
                RowNumber = e.RowNumber,
                Column = e.Column,
                Message = e.Message
            }).ToList() };

        var tableTriggers = new Dictionary<string, List<string>>
        {
            ["DEP_MARCHE"] = await GetTableTriggersAsync("DEP_MARCHE", ct),
            ["DEP_ORDRESERVICE"] = await GetTableTriggersAsync("DEP_ORDRESERVICE", ct),
            ["CPT_ENGAGEMENT"] = await GetTableTriggersAsync("CPT_ENGAGEMENT", ct),
            ["DEP_RECEPTION"] = await GetTableTriggersAsync("DEP_RECEPTION", ct),
            ["DEP_FACTURES"] = await GetTableTriggersAsync("DEP_FACTURES", ct)
            // Ajouter d'autres tables si nécessaire
        };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Désactiver tous les triggers
            foreach (var table in tableTriggers)
            {
                foreach (var trigger in table.Value)
                {
                    await _db.Database.ExecuteSqlRawAsync($"DISABLE TRIGGER [{trigger}] ON {table.Key}", ct);
                    Console.WriteLine($"[DEBUG] Trigger désactivé: {table.Key}.{trigger}");
                }
            }

            var parentMap = new Dictionary<string, (DepMarche Entity, string codeFils, string codeH)>(StringComparer.OrdinalIgnoreCase);
            var inserted = 0;

            // Compteurs globaux
            var codeFilsCounter = await GetMaxCodeFilsAsync(ct);
            var codeHCounter = await GetMaxCodeHAsync(ct);

            // Groupement préalable des bordereaux par N° Dépense pour un accès rapide
            var bordereauxByDepense = wbData.Bordereaux
                .GroupBy(b => b.NumeroDepense, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            // --- BOUCLE PRINCIPALE (Parent + Enfants imbriqués) ---
            foreach (var fg in wbData.FicheGenerale)
            {
                var existingDepense = await _db.DepMarches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.NumeroMarche == fg.NumeroDepense, ct);

                if (existingDepense != null)
                {
                    errors.Add(new ImportErrorDto { Sheet = "Fiche Générale", RowNumber = fg.ExcelRow, Column = "N° Dépense", Message = "Le N° Dépense existe déjà dans la table DEP_MARCHE." });
                    continue;
                }

                var fk = await _lookup.ResolveFicheGeneraleCodesAsync(fg, ct, errors);
                if (errors.Any()) continue;

                var depMarche = _ficheMapper.MapFicheGeneraleToDepMarche(
                    fg,
                    fk.CodeTypeContract, fk.CodeFournisseur,
                    fk.CodeDevise, fk.CodeTypeBudget, fk.CodeTypePrestation,
                    fk.CodeNaturePrestation, fk.CodeCentreGestionnaire,
                    fk.CodeMaitreOuvrage, fk.CodeModePaiement, fk.CodeBanquePaiement,
                    fk.CodePersonneHabilitee, fk.DelaiExecution, fk.DelaiGarantie,
                    fk.UniteDelaiExecution, fk.UniteDelaiGarantie
                );

                _db.DepMarches.Add(depMarche);
                try
                {
                    await _db.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    var dbError = MapExceptionToDbError(ex);
                    AddDbErr(dbErrors, dbError.Message, dbError.ErrorType, dbError.SqlErrorCode, dbError.EntityName);
                }
                inserted++;

                var codeMarche = depMarche.CodeMarche;
                _db.DepMarcheCentres.Add(_ficheMapper.MapToDepMarcheCentre(codeMarche, fk.CodeCentreBeneficiaire));

                var suivi = _ficheMapper.MapToDepChargeSuivi(fg, codeMarche, fk.CodeAgentChargeSuivi);
                if (suivi != null) _db.DepChargeSuivis.Add(suivi);

                // A. GÉNÉRATION DU CODE PARENT (DepMarchesH)
                codeFilsCounter++;
                var parentCodeFils = codeFilsCounter.ToString().PadLeft(8, '0');

                codeHCounter++;
                var parentCodeH = $"1.{codeHCounter:000}";

                var typeDepense = await _db.DepTypeContrats
                    .Where(x => x.CodeTypeContrat == fk.CodeTypeContract)
                    .Select(x => x.LibelleTypeContrat)
                    .FirstOrDefaultAsync(ct);

                var countBordereaux = bordereauxByDepense.TryGetValue(fg.NumeroDepense, out var items) ? items.Count : 0;

                var depNode = _ficheMapper.MapDepenseNodeToDepMarchesH(
                    parentCodeFils, fg.NumeroDepense, "00000000", parentCodeH,
                    countBordereaux,
                    typeDepense,
                    codeMarche
                );

                depNode.RefUtil = parentCodeFils;
                _db.DepMarchesH.Add(depNode);

                parentMap[fg.NumeroDepense] = (depMarche, parentCodeFils, parentCodeH);

                // B. GÉNÉRATION IMMÉDIATE DES ENFANTS (Bordereaux de ce Parent)
                if (bordereauxByDepense.TryGetValue(fg.NumeroDepense, out var listBordereaux))
                {
                    var refUtilLocal = 0;
                    var codeHLocal = 0;

                    foreach (var br in listBordereaux)
                    {
                        var product = await _lookup.ResolveProduitCodeAsync(br.DesignationProduit, ct);
                        if (product == null)
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Borderaux de prix", RowNumber = br.ExcelRow, Column = "Désignation Produit", Message = "Produit inexistant." });
                            continue;
                        }

                        refUtilLocal++;
                        var refUtil = refUtilLocal.ToString();

                        codeHLocal++;
                        var codeHProduit = $"{parentCodeH}.{codeHLocal}";

                        // Incrémentation globale du CODE_FILS pour l'enfant (A la suite directe du Parent !)
                        codeFilsCounter++;
                        var codeFilsProduit = codeFilsCounter.ToString().PadLeft(8, '0');

                        // UNITE
                        var uniteProduit = _db.GenUnites
                            .Where(x => x.CodeUnite.ToString() == product.Value.unite)
                            .Select(x => x.Designation)
                            .FirstOrDefault();

                        var entity = _bordMapper.MapProduitNodeToDepMarchesH(
                            br,
                            codeFilsProduit,   // CODE_FILS séquentiel immédiat
                            parentCodeFils,    // CODE_PERE du parent tout juste créé
                            codeHProduit,
                            product.Value.codeProduit,
                            uniteProduit,
                            refUtil,
                            refUtil.PadLeft(5, '0')
                        );

                        _db.DepMarchesH.Add(entity);

                        // Calculs des montants
                        var montantHtProduit = entity.PrixUnit * entity.Quantite;
                        var montantTtcProduit = entity.PrixTtc;
                        var montantRemiseProduit = montantTtcProduit;
                        var tauxRabais = 0.0m;
                        var rabais = 0.0m;

                        if (entity.Remise == "1")
                        {
                            tauxRabais = (decimal)(depMarche.TauxRabais ?? 0);
                            rabais = (decimal)(montantTtcProduit * tauxRabais / 100 ?? 0);
                            montantRemiseProduit -= rabais;
                        }

                        depMarche.MontantMarcheHt = (depMarche.MontantMarcheHt ?? 0) + montantHtProduit;
                        depMarche.MontantMarcheTtc = (depMarche.MontantMarcheTtc ?? 0) + montantTtcProduit;
                        depMarche.MtMarcheRabais = (depMarche.MtMarcheRabais ?? 0) + montantRemiseProduit;

                        var tarifFrs = _bordMapper.MapToGenTarifFournisseur(br, tauxRabais, depMarche.CodeFournisseur, product.Value.codeProduit);
                        _db.GenTarifFournisseurs.Add(tarifFrs);
                    }
                    depMarche.TauxTva = (float?)((depMarche.MontantMarcheTtc - depMarche.MontantMarcheHt) / depMarche.MontantMarcheHt) * 100;
                }
            }

            // Gestion globale des erreurs
            if (errors.Any())
            {
                await tx.RollbackAsync(ct);
                return new ImportResultDto { Success = false, TotalRows = wbData.FicheGenerale.Count, InsertedRows = 0, Errors = errors };
            }

            if (dbErrors.Any())
            {
                return new ImportResultDto
                {
                    Success = false,
                    TotalRows = wbData.FicheGenerale.Count,
                    InsertedRows = 0,
                    Errors = dbErrors.Select(e => new ImportErrorDto
                    {
                        Sheet = e.Sheet,
                        RowNumber = e.RowNumber,
                        Column = e.Column,
                        Message = e.Message
                    }).ToList()
                };
            }

            //Ordres de service
            foreach (var os in wbData.OrdresService)
            {
                if (!parentMap.TryGetValue(os.NumeroDepense, out var parent))
                {
                    errors.Add(new ImportErrorDto { Sheet = "Ordre de service", RowNumber = os.ExcelRow, Column = "N° Dépense", Message = "Introuvable dans Fiche Générale." });
                    continue;
                }

                var numOS = await _db.DepOrdreServices
                    .Where(x => x.CodeMarche == parent.Entity.CodeMarche)
                    .CountAsync(ct);
                numOS++;

                _db.DepOrdreServices.Add(_osMapper.MapToDepOrdreService(os, parent.Entity.CodeMarche, numOS));
            }

            // Engagements
            if (options.IncludeEngagement)
            {
                decimal? codeLigneEng = null;
                CptEngagement? engagementCourant = null;
                string? numDepense = null;

                foreach (var eng in wbData.Engagements)
                {
                    // 1. Détection : Est-ce une ligne principale (nouvel engagement) ou une ligne fille ?
                    bool estNouvelEngagement = !string.IsNullOrWhiteSpace(eng.CategorieBudget) && eng.DateEngagement.HasValue;

                    if (estNouvelEngagement)
                    {
                        if (!parentMap.TryGetValue(eng.NumeroDepense!, out var parent))
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Engagement", RowNumber = eng.ExcelRow, Column = "N° Dépense", Message = "Introuvable dans Fiche Générale." });
                            continue;
                        }

                        var fkHeader = await _lookup.ResolveEngagementCodesAsync(eng, ct, errors);
                        if (errors.Any()) continue;

                        var numOrdre = await GenererProchainNumOrdreAsync(ct);

                        engagementCourant = _engMapper.MapToCptEngagement(
                            eng, fkHeader.CodeTypeCredit, fkHeader.Annee, numOrdre, fkHeader.CodeTypeContract);

                        _db.CptEngagements.Add(engagementCourant);
                        try
                        {
                            await _db.SaveChangesAsync(ct);
                            codeLigneEng = engagementCourant.CodeEngagement;
                            inserted++;
                        }
                        catch (Exception ex)
                        {
                            var dbError = MapExceptionToDbError(ex);
                            AddDbErr(dbErrors, dbError.Message, dbError.ErrorType, dbError.SqlErrorCode, dbError.EntityName);
                            continue;
                        }
                        numDepense= eng.NumeroDepense;
                    }

                    // 2. Vérification qu'un engagement parent existe pour les lignes secondaires
                    if (!codeLigneEng.HasValue || engagementCourant == null)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Engagement", RowNumber = eng.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Aucun engagement parent associé." });
                        continue;
                    }

                    if (eng.NumeroDepense != numDepense)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Engagement", RowNumber = eng.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Le N° Dépense ne correspond pas à l'engagement parent." });
                        continue;
                    }

                    // 3. Résolution des codes budgétaires propres à cette ligne
                    var fk = await _lookup.ResolveEngagementCodesAsync(eng, ct, errors);
                    if (errors.Any()) continue;

                    // 4. Mises à jour budgétaires & Insertion de la ligne (CptEngagementLigne)
                    var dotationLigne = await _db.BdgDotationLigneBudgetaires
                        .FirstOrDefaultAsync(x => x.CodeLigne == fk.CodeLigne && x.Annee == fk.Annee, ct);

                    if (dotationLigne == null)
                    {
                        errors.Add(new ImportErrorDto
                        {
                            Sheet = "Engagement",
                            RowNumber = eng.ExcelRow,
                            Column = "Lignes budgétaires",
                            Message = $"Dotation introuvable pour la ligne budgétaire {fk.CodeLigne} / exercice {fk.Annee}."
                        });
                        continue;
                    }

                    var dotationCreditSupp = await _db.BdgDotationCreditSupps
                        .FirstOrDefaultAsync(x => x.CodeLigne == fk.CodeLigne
                                                && x.CodeTypeCredit == fk.CodeTypeCredit
                                                && x.Annee == ((int)fk.Annee).ToString(), ct);

                    if (dotationCreditSupp == null)
                    {
                        errors.Add(new ImportErrorDto
                        {
                            Sheet = "Engagement",
                            RowNumber = eng.ExcelRow,
                            Column = "Lignes budgétaires",
                            Message = $"Crédit supplémentaire introuvable pour la ligne {fk.CodeLigne} / type crédit {fk.CodeTypeCredit} / exercice {fk.Annee}."
                        });
                        continue;
                    }

                    // Incrémentation compteur
                    dotationLigne.CompteurCp = (dotationLigne.CompteurCp ?? 0) + 1;

                    var engagementLigne = _engMapper.MapToCptEngagementLigne(
                        eng, codeLigneEng.Value, fk.CodeDepense, fk.CodeLigne, fk.DisponibleCP, (decimal)fk.DisponibleCE,
                        (decimal)fk.CreditEngCP, (decimal)fk.CreditEngCE, (decimal)(dotationLigne.CompteurCp ?? 0));

                    _db.CptEngagementLignes.Add(engagementLigne);

                    // Sans validation
                    dotationLigne.CreditPreEngageCp = fk.CreditPreEngCP + (double)(engagementLigne.MontantEngageCp ?? 0.0m);
                    dotationLigne.CreditPreEngageCe = fk.CreditPreEngCE + (double)(engagementLigne.MontantEngageCe ?? 0.0m);

                    dotationCreditSupp.CreditPreEngageCp = (decimal)fk.CreditPreEngCP + (engagementLigne.MontantEngageCp ?? 0.0m);
                    dotationCreditSupp.CreditPreEngageCe = (decimal)fk.CreditPreEngCE + (engagementLigne.MontantEngageCe ?? 0.0m);

                    // Ajustement des crédits (apres validation)
                    if (engagementCourant.Accorde == 1)
                    {
                        dotationLigne.CreditPreEngageCp = fk.CreditPreEngCP - (double)(engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationLigne.CreditPreEngageCe = fk.CreditPreEngCE - (double)(engagementLigne.MontantEngageCe ?? 0.0m);
                        dotationLigne.CreditDisponibleCp = fk.DisponibleCP - (engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationLigne.CreditDisponibleCe = fk.DisponibleCE - (double)(engagementLigne.MontantEngageCe ?? 0.0m);
                        dotationLigne.CreditEngageCp = fk.CreditEngCP + (double)(engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationLigne.CreditEngageCe = fk.CreditEngCE + (double)(engagementLigne.MontantEngageCe ?? 0.0m);

                        dotationCreditSupp.CreditPreEngageCp = (decimal)fk.CreditPreEngCP - (engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationCreditSupp.CreditPreEngageCe = (decimal)fk.CreditPreEngCE - (engagementLigne.MontantEngageCe ?? 0.0m);
                        dotationCreditSupp.CreditDisponibleCp = fk.DisponibleCP - (engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationCreditSupp.CreditDisponibleCe = (decimal)fk.DisponibleCE - (engagementLigne.MontantEngageCe ?? 0.0m);
                        dotationCreditSupp.CreditEngageCp = (decimal)fk.CreditEngCP + (engagementLigne.MontantEngageCp ?? 0.0m);
                        dotationCreditSupp.CreditEngageCe = (decimal)fk.CreditEngCE + (engagementLigne.MontantEngageCe ?? 0.0m);
                    }
                }
            }

            // Livraison
            if (options.IncludeLivraison)
            {
                decimal? codeReleve = null;
                DepReleveMarche? releveCourant = null;
                string? numDepense = null;

                foreach (var liv in wbData.Livraisons)
                {
                    // 1. Détection : Est-ce une ligne principale (nouvelle livraison) ou une ligne fille ?
                    bool estNouvelReleve = !string.IsNullOrWhiteSpace(liv.NumeroLivraison) && liv.DateLivraison.HasValue;

                    if (estNouvelReleve)
                    {
                        var existingBL = await _db.DepReleveMarches
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.ReferenceLivraison == liv.NumeroLivraison, ct);

                        if (existingBL != null)
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "N° BL", Message = "Le N° BL existe déjà dans la table DEP_RELEVE_MARCHE." });
                            continue;
                        }

                        if (!parentMap.TryGetValue(liv.NumeroDepense, out var parent))
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "N° Dépense", Message = "Introuvable dans Fiche Générale." });
                            continue;
                        }

                        var fkHeader = await _lookup.ResolveLivraisonCodesAsync(liv, ct, errors);
                        if (errors.Any()) continue;

                        var numOrdreReleve = await _db.DepReleveMarches
                            .Where(x => x.CodeMarche == fkHeader.CodeDepense)
                            .CountAsync(ct);
                        numOrdreReleve++;

                        var numReleve = $"{numOrdreReleve.ToString()}/{liv.NumeroDepense}";

                        releveCourant = _livMapper.MapToDepReleveMarche(
                            liv, numOrdreReleve, numReleve, fkHeader.CodeDepense, fkHeader.CodeResponsable, fkHeader.CodeMagasin, fkHeader.CodeMagasinier);

                        _db.DepReleveMarches.Add(releveCourant);
                        try
                        {
                            await _db.SaveChangesAsync(ct);
                            codeReleve = releveCourant.CodeReleve;
                            inserted++;
                        }
                        catch (Exception ex)
                        {
                            var dbError = MapExceptionToDbError(ex);
                            AddDbErr(dbErrors, dbError.Message, dbError.ErrorType, dbError.SqlErrorCode, dbError.EntityName);
                            continue;
                        }
                        numDepense = liv.NumeroDepense;
                    }

                    // 2. Vérification qu'un engagement parent existe pour les lignes secondaires
                    if (!codeReleve.HasValue || releveCourant == null)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Aucun relevé parent associé." });
                        continue;
                    }

                    if (liv.NumeroDepense != numDepense)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Le N° Dépense ne correspond pas au relevé parent." });
                        continue;
                    }

                    // 3. Résolution des produits propres à cette ligne
                    var fk = await _lookup.ResolveLivraisonCodesAsync(liv, ct, errors);
                    if (errors.Any()) continue;

                    var livraisonLigne = _livMapper.MapToDepLigneReleveMarche(
                        liv, (decimal)codeReleve, fk.CodeFilsProduit, fk.TauxTva, fk.Quantite);

                    _db.DepLigneReleveMarches.Add(livraisonLigne);

                    var prixU = await _db.DepMarchesH
                        .Where(x => x.CodeFils == fk.CodeFilsProduit)
                        .Select(x => x.PrixUnit)
                        .FirstOrDefaultAsync(ct);

                    var montantHt = prixU * (livraisonLigne.QteLiv ?? 0);
                    var montantTtc = montantHt * (1 + (livraisonLigne.TauxTva ?? 0) / 100);

                    releveCourant.MtReleveHt = (releveCourant.MtReleveHt ?? 0) + montantHt;
                    releveCourant.MtReleveTtc = (releveCourant.MtReleveTtc ?? 0) + montantTtc;

                    var ventilReleve = _livMapper.MapToDepVentilQuantiteReleve(
                        liv, fk.CodeCentreResp, (decimal)codeReleve, fk.CodeFilsProduit);

                    _db.DepVentilQuantiteReleves.Add(ventilReleve);
                }
            }

            // Reception
            if (options.IncludeReception)
            {
                decimal? codeReception = null;
                DepReception? receptionCourant = null;
                string? numDepense = null;

                foreach (var rec in wbData.Receptions)
                {
                    // 1. Détection : Est-ce une ligne principale (nouvelle livraison) ou une ligne fille ?
                    bool estNouvelReception = !string.IsNullOrWhiteSpace(rec.NatureReception) 
                        && !string.IsNullOrWhiteSpace(rec.TypeReception) && rec.DateReception.HasValue;

                    if (estNouvelReception)
                    {
                        if (!parentMap.TryGetValue(rec.NumeroDepense, out var parent))
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Réception", RowNumber = rec.ExcelRow, Column = "N° Dépense", Message = "Introuvable dans Fiche Générale." });
                            continue;
                        }

                        var fkHeader = await _lookup.ResolveReceptionCodesAsync(rec, ct, errors);
                        if (errors.Any()) continue;

                        var numReception = await _db.DepReceptions
                            .Where(x => x.CodeMarche == fkHeader.CodeDepense)
                            .CountAsync(ct);
                        numReception++;

                        receptionCourant = _recMapper.MapToDepReception(
                            rec, numReception, fkHeader.CodeDepense, (decimal)fkHeader.CodeNature, (decimal)fkHeader.CodeType);

                        _db.DepReceptions.Add(receptionCourant);
                        try
                        {
                            await _db.SaveChangesAsync(ct);
                            codeReception = receptionCourant.CodeReception;
                            inserted++;
                        }
                        catch (Exception ex)
                        {
                            var dbError = MapExceptionToDbError(ex);
                            AddDbErr(dbErrors, dbError.Message, dbError.ErrorType, dbError.SqlErrorCode, dbError.EntityName);
                            continue;
                        }
                        numDepense = rec.NumeroDepense;

                        InstanceProcessus instance = _recMapper.MapToInstanceProcessus((decimal)codeReception, receptionCourant.Valider ?? 0);
                        _db.InstanceProcessuses.Add(instance);
                    }

                    // 2. Vérification qu'un engagement parent existe pour les lignes secondaires
                    if (!codeReception.HasValue || receptionCourant == null)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Réception", RowNumber = rec.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Aucun réception parent associé." });
                        continue;
                    }

                    if (rec.NumeroDepense != numDepense)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Réception", RowNumber = rec.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Le N° Dépense ne correspond pas au réception parent." });
                        continue;
                    }

                    // 3. Résolution des produits propres à cette ligne
                    var fk = await _lookup.ResolveReceptionCodesAsync(rec, ct, errors);
                    if (errors.Any()) continue;

                    var receptionLigne = _recMapper.MapToDepReceptionReleve((int)codeReception, (decimal)fk.CodeReleve);

                    _db.DepReceptionReleves.Add(receptionLigne);

                    DepReleveMarche? livraison = await _db.DepReleveMarches.FirstOrDefaultAsync(x => x.CodeReleve == fk.CodeReleve, ct);
                    if (livraison != null)
                    {
                        livraison.IsReceptionned = 1;
                    }
                }
            }

            // Facture
            if (options.IncludeFacture)
            {
                decimal? codeFacture = null;
                DepFacture? factureCourante = null;
                DepMontantsFacture? montantsFacture = null;
                DepAllMontantsFacture? allMontantsFacture = null;
                string? numDepense = null;
                decimal? codeAv = await GetMaxCodeAvancementAsync(ct);

                var produitsAggreges = new Dictionary<string, (
                    FactureRowDto fac,
                    decimal QteLivree, decimal QteAcceptee, decimal? CodeProduit, decimal? PrixUnite,
                    decimal? TauxTva, string? Remise, decimal? CodeCentreResp,
                    decimal TauxRsFacture, decimal TauxRemiseFacture // <-- AJOUT
                )>(StringComparer.OrdinalIgnoreCase);

                // Helper pour sauvegarder les données agrégées de la facture précédente
                void SauvegarderFacturePrecedente()
                {
                    if (codeFacture.HasValue && codeAv.HasValue && produitsAggreges.Count > 0)
                    {
                        foreach (var kvp in produitsAggreges)
                        {
                            var (fac, qteLivree, qteAcceptee, codeProduit, prixUnite, tauxTva, remise, codeCentreResp, tauxRs, tauxRemise) = kvp.Value;
                            var codeFils = kvp.Key;

                            // 1. Avancement
                            var avancement = _facMapper.MapToDepAvancementFacture(
                                (decimal)codeAv, (decimal)codeFacture, codeFils, qteLivree, qteAcceptee);
                            _db.DepAvancementFactures.Add(avancement);

                            // 2. Ventilation (Maintenant qu'on a tous les taux, c'est propre)
                            decimal montantHtTotal = (prixUnite ?? 0m) * qteLivree;
                            decimal remiseTotale = 0;
                            if (remise == "1")
                                remiseTotale = (decimal)(montantHtTotal * (tauxTva / 100m) * (tauxRemise / 100m));

                            var ventils = _facMapper.MapToDepVentilMntsFacture(
                                fac,
                                (decimal)codeFacture,
                                codeProduit ?? 0m,
                                remiseTotale,
                                montantHtTotal,
                                codeFils,
                                codeCentreResp,
                                tauxRs,
                                tauxTva ?? 0m);
                            _db.DepVentilMntsFactures.Add(ventils);
                        }
                    }
                }

                foreach (var fac in wbData.Factures)
                {
                    // 1. Détection : Est-ce une ligne principale (nouvelle livraison) ou une ligne fille ?
                    bool estNouvelFacture = !string.IsNullOrWhiteSpace(fac.NumeroFactureFournisseur) && fac.DateFacture.HasValue && fac.DateDepotFacture.HasValue;

                    if (estNouvelFacture)
                    {
                        // 1. Sauvegarder les agrégations de la facture PRÉCÉDENTE avant de passer à la nouvelle
                        SauvegarderFacturePrecedente();

                        // 2. Réinitialiser le dictionnaire pour la NOUVELLE facture
                        produitsAggreges.Clear();

                        if (!parentMap.TryGetValue(fac.NumeroDepense, out var parent))
                        {
                            errors.Add(new ImportErrorDto { Sheet = "Facture", RowNumber = fac.ExcelRow, Column = "N° Dépense", Message = "Introuvable dans Fiche Générale." });
                            continue;
                        }

                        codeAv++;

                        var fkHeader = await _lookup.ResolveFactureCodesAsync(fac, ct, errors);
                        if (errors.Any()) continue;

                        var numOrdreFacture = await _db.DepFactures
                            .Where(x => x.CodeMarche == fkHeader.CodeDepense)
                            .CountAsync(ct);
                        numOrdreFacture++;

                        var numFacture = $"F {numOrdreFacture.ToString()}/{fac.NumeroDepense}";

                        factureCourante = _facMapper.MapToDepFacture(
                            fac, numOrdreFacture, numFacture, fkHeader.CodeDepense, fkHeader.Periode, fkHeader.CodeFournisseur,
                            fkHeader.MontantFactureHT, fkHeader.MontantFactureTTC);

                        _db.DepFactures.Add(factureCourante);
                        try
                        {
                            await _db.SaveChangesAsync(ct);
                            codeFacture = factureCourante.CodeFacture;
                            inserted++;
                        }
                        catch (Exception ex)
                        {
                            var dbError = MapExceptionToDbError(ex);
                            AddDbErr(dbErrors, dbError.Message, dbError.ErrorType, dbError.SqlErrorCode, dbError.EntityName);
                            continue;
                        }
                        numDepense = fac.NumeroDepense;

                        montantsFacture = _facMapper.MapToDepMontantsFacture(fac, (decimal)codeFacture);
                        allMontantsFacture = _facMapper.MapToDepAllMontantsFacture(fac, (decimal)codeFacture);

                        _db.DepMontantsFactures.Add(montantsFacture);
                        _db.DepAllMontantsFactures.Add(allMontantsFacture);
                    }

                    // 2. Vérification qu'un engagement parent existe pour les lignes secondaires
                    if (!codeFacture.HasValue || factureCourante == null)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Facture", RowNumber = fac.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Aucune facture parent associé." });
                        continue;
                    }

                    if (fac.NumeroDepense != numDepense)
                    {
                        errors.Add(new ImportErrorDto { Sheet = "Facture", RowNumber = fac.ExcelRow, Column = "N° Dépense", Message = "Ligne orpheline : Le N° Dépense ne correspond pas au facture parent." });
                        continue;
                    }

                    // 3. Résolution des produits propres à cette ligne
                    var fk = await _lookup.ResolveFactureCodesAsync(fac, ct, errors);
                    if (errors.Any()) continue;

                    // Insertion infos livraisons de facture
                    var factureLigne = _facMapper.MapToDepFactureReleve((decimal)codeFacture, fk.CodeReleve);
                    _db.DepFactureReleves.Add(factureLigne);

                    DepReleveMarche? livraison = await _db.DepReleveMarches.FirstOrDefaultAsync(x => x.CodeReleve == fk.CodeReleve, ct);
                    if (livraison != null)
                    {
                        livraison.IsFactured = 1;
                    }

                    // Insertion montants facture
                    // --- ÉTAPE 1 : Normalisation des taux et montants de base ---
                    decimal tauxTva = (fk.TauxTvaFacture ?? 0m) / 100m;
                    decimal tauxRemise = (fk.TauxRemiseFacture ?? 0m) / 100m;
                    decimal tauxRs = (fk.TauxRsFacture ?? 0m) / 100m;

                    decimal mntHt = fk.MontantFactureHT ?? 0m;
                    decimal mntTtc = fk.MontantFactureTTC ?? 0m;

                    // Calculs intermédiaires réutilisables
                    decimal mntTva = mntHt * tauxTva;
                    decimal mntRabaisHt = mntHt * tauxRemise;
                    decimal mntRabaisTva = mntRabaisHt * tauxTva;
                    decimal mntRabaisTtc = mntTtc * tauxRemise;
                    decimal mntRs = mntHt * tauxRs;
                    decimal mntRsTva = mntRs * tauxTva;

                    decimal mntApresRabaisHt = mntHt - mntRabaisHt;
                    decimal mntApresRabaisTva = mntApresRabaisHt * tauxTva;
                    decimal mntApresRabaisTtc = mntApresRabaisHt * (1m + tauxTva);

                    // --- ÉTAPE 2 : Affectation aux entités ---

                    // DepMontantsFacture
                    montantsFacture.CumulActuelHt = (montantsFacture.CumulActuelHt ?? 0m) + mntHt;
                    montantsFacture.CumulActuelTva = (montantsFacture.CumulActuelTva ?? 0m) + mntTva;
                    montantsFacture.CumulActuelTtc = (montantsFacture.CumulActuelTtc ?? 0m) + mntTtc;

                    montantsFacture.RabaisHt = (montantsFacture.RabaisHt ?? 0m) + mntRabaisHt;
                    montantsFacture.RabaisTva = (montantsFacture.RabaisTva ?? 0m) + mntRabaisTva;
                    montantsFacture.RabaisTtc = (montantsFacture.RabaisTtc ?? 0m) + mntRabaisTtc;

                    montantsFacture.TravauxMoisHt = (montantsFacture.TravauxMoisHt ?? 0m) + mntHt;
                    montantsFacture.TravauxMoisTva = (montantsFacture.TravauxMoisTva ?? 0m) + mntTva;
                    montantsFacture.TravauxMoisTtc = (montantsFacture.TravauxMoisTtc ?? 0m) + mntTtc;

                    montantsFacture.MontantFactureHt = (montantsFacture.MontantFactureHt ?? 0m) + mntHt;
                    montantsFacture.MontantFactureTva = (montantsFacture.MontantFactureTva ?? 0m) + mntTva;
                    montantsFacture.MontantFactureTtc = (montantsFacture.MontantFactureTtc ?? 0m) + mntTtc;
                    montantsFacture.MontanPayeTtc = (montantsFacture.MontanPayeTtc ?? 0m) + mntTtc;

                    montantsFacture.MontantFactureRs = (montantsFacture.MontantFactureRs ?? 0m) + mntRs;
                    montantsFacture.MontantFactureRsTva = (montantsFacture.MontantFactureRsTva ?? 0m) + mntRsTva;
                    montantsFacture.MontantFactureBaseRsTva = (montantsFacture.MontantFactureBaseRsTva ?? 0m) + mntTva;
                    montantsFacture.MtFactureHtBaseRsIrIs = (montantsFacture.MtFactureHtBaseRsIrIs ?? 0m) + mntHt;
                    montantsFacture.MontantFactureHtBaseRsTva = (montantsFacture.MontantFactureHtBaseRsTva ?? 0m) + mntHt;

                    // DepAllMontantsFacture
                    allMontantsFacture.CumulActuelHt = (allMontantsFacture.CumulActuelHt ?? 0m) + mntHt;
                    allMontantsFacture.CumulActuelTva = (allMontantsFacture.CumulActuelTva ?? 0m) + mntTva;
                    allMontantsFacture.CumulActuelTtc = (allMontantsFacture.CumulActuelTtc ?? 0m) + mntTtc;

                    allMontantsFacture.RabaisHt = (allMontantsFacture.RabaisHt ?? 0m) + mntRabaisHt;
                    allMontantsFacture.RabaisTva = (allMontantsFacture.RabaisTva ?? 0m) + mntRabaisTva;
                    allMontantsFacture.RabaisTtc = (allMontantsFacture.RabaisTtc ?? 0m) + mntRabaisTtc;

                    allMontantsFacture.CumulActApresRabaisHt = (allMontantsFacture.CumulActApresRabaisHt ?? 0m) + mntApresRabaisHt;
                    allMontantsFacture.CumulActApresRabaisTva = (allMontantsFacture.CumulActApresRabaisTva ?? 0m) + mntApresRabaisTva;
                    allMontantsFacture.CumulActApresRabaisTtc = (allMontantsFacture.CumulActApresRabaisTtc ?? 0m) + mntApresRabaisTtc;

                    allMontantsFacture.TravauxMoisHt = (allMontantsFacture.TravauxMoisHt ?? 0m) + mntHt;
                    allMontantsFacture.TravauxMoisTva = (allMontantsFacture.TravauxMoisTva ?? 0m) + mntTva;
                    allMontantsFacture.TravauxMoisTtc = (allMontantsFacture.TravauxMoisTtc ?? 0m) + mntTtc;

                    allMontantsFacture.MontantFactureHt = (allMontantsFacture.MontantFactureHt ?? 0m) + mntHt;
                    allMontantsFacture.MontantFactureTva = (allMontantsFacture.MontantFactureTva ?? 0m) + mntTva;
                    allMontantsFacture.MontantFactureTtc = (allMontantsFacture.MontantFactureTtc ?? 0m) + mntTtc;
                    allMontantsFacture.MontanPayeTtc = (allMontantsFacture.MontanPayeTtc ?? 0m) + mntTtc;

                    allMontantsFacture.MontantFactureRs = (allMontantsFacture.MontantFactureRs ?? 0m) + mntRs;
                    allMontantsFacture.CumulActuelRs = (allMontantsFacture.CumulActuelRs ?? 0m) + mntRs;
                    allMontantsFacture.TravauxMoisRs = (allMontantsFacture.TravauxMoisRs ?? 0m) + mntRs;
                    allMontantsFacture.CumulActApresRabaisRs = (allMontantsFacture.CumulActApresRabaisRs ?? 0m) + mntRs;

                    allMontantsFacture.MontantFactureRsTva = (allMontantsFacture.MontantFactureRsTva ?? 0m) + mntRsTva;
                    allMontantsFacture.MontantFactureBaseRsTva = (allMontantsFacture.MontantFactureBaseRsTva ?? 0m) + mntTva;
                    allMontantsFacture.MtFactureHtBaseRsIrIs = (allMontantsFacture.MtFactureHtBaseRsIrIs ?? 0m) + mntHt;
                    allMontantsFacture.MontantFactureHtBaseRsTva = (allMontantsFacture.MontantFactureHtBaseRsTva ?? 0m) + mntHt;

                    // ✅ AGRÉGATION DES PRODUITS DANS LE DICTIONNAIRE
                    foreach (var prod in fk.Produits)
                    {
                        if (string.IsNullOrWhiteSpace(prod.CodeFilsProduit)) continue;

                        decimal qteLivree = prod.QuantiteLivreeProduit ?? 0m;
                        decimal qteAcceptee = prod.QuantiteAcepteeProduit ?? 0m;

                        if (produitsAggreges.TryGetValue(prod.CodeFilsProduit, out var existing))
                        {
                            // Le produit existe déjà dans une autre livraison de cette facture : on ADDITIONNE
                            produitsAggreges[prod.CodeFilsProduit] = (
                                existing.fac,
                                existing.QteLivree + qteLivree,
                                existing.QteAcceptee + qteAcceptee,
                                existing.CodeProduit,
                                existing.PrixUnite,                   
                                existing.TauxTva,
                                existing.Remise,
                                existing.CodeCentreResp,      
                                existing.TauxRsFacture,
                                existing.TauxRemiseFacture
                            );
                        }
                        else
                        {
                            // Premier fois qu'on voit ce produit pour cette facture
                            produitsAggreges[prod.CodeFilsProduit] = (
                                fac,
                                qteLivree, qteAcceptee, prod.CodeProduit, prod.PrixUnite, prod.TauxTvaProduit,
                                prod.RemiseProduit, prod.CodeCentreResp,
                                fk.TauxRsFacture ?? 0m, fk.TauxRemiseFacture ?? 0m
                            );
                        }
                    }
                }
                SauvegarderFacturePrecedente();
            }

            if (errors.Any() || dbErrors.Any())
            {
                await tx.RollbackAsync(ct);

                var allErrors = new List<ImportErrorDto>(errors);
                allErrors.AddRange(dbErrors.Select(e => new ImportErrorDto
                {
                    Sheet = e.Sheet,
                    RowNumber = e.RowNumber,
                    Column = e.Column,
                    Message = e.Message
                }));

                return new ImportResultDto
                {
                    Success = false,
                    TotalRows = wbData.FicheGenerale.Count,
                    InsertedRows = 0,
                    Errors = allErrors
                };
            }

            inserted += await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            // Réactiver tous les triggers
            foreach (var table in tableTriggers)
            {
                foreach (var trigger in table.Value)
                {
                    await _db.Database.ExecuteSqlRawAsync($"ENABLE TRIGGER [{trigger}] ON {table.Key}", ct);
                    Console.WriteLine($"[DEBUG] Trigger réactivé: {table.Key}.{trigger}");
                }
            }

            return new ImportResultDto { Success = true, TotalRows = wbData.FicheGenerale.Count, InsertedRows = inserted, Errors = new() };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail("Database", 0, "-", ex.Message);
        }
    }

    private async Task<List<string>> GetTableTriggersAsync(string tableName, CancellationToken ct)
    {
        var triggers = new List<string>();
        var connection = _db.Database.GetDbConnection();
        var wasClosed = connection.State == System.Data.ConnectionState.Closed;

        try
        {
            if (wasClosed)
                await connection.OpenAsync(ct);

            using var cmd = connection.CreateCommand();

            // ✅ FIX 1 : Attacher la transaction EF Core en cours pour éviter les blocages (deadlocks)
            var currentTransaction = _db.Database.CurrentTransaction?.GetDbTransaction();
            if (currentTransaction != null)
            {
                cmd.Transaction = currentTransaction;
            }

            // ✅ FIX 2 : Augmenter le timeout à 60 secondes pour les requêtes sur les tables système
            cmd.CommandTimeout = 60;

            // ✅ FIX 3 : Requête hautement optimisée avec JOIN sur sys.objects et NOLOCK
            // C'est beaucoup plus rapide que d'appeler OBJECT_NAME() pour chaque ligne
            cmd.CommandText = @"
            SELECT t.name 
            FROM sys.triggers t WITH (NOLOCK)
            INNER JOIN sys.objects o WITH (NOLOCK) ON t.parent_id = o.object_id
            WHERE t.parent_class = 1 
              AND o.name = @TableName
              AND t.is_disabled = 0";

            var param = cmd.CreateParameter();
            param.ParameterName = "@TableName";
            param.Value = tableName;
            cmd.Parameters.Add(param);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                triggers.Add(reader.GetString(0));
            }
        }
        finally
        {
            if (wasClosed)
                await connection.CloseAsync();
        }

        return triggers;
    }

    // ---------------- Validations ----------------
    private static void ValidateFiche(FicheGeneraleRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Fiche Générale", RowNumber = r.ExcelRow, Column = c, Message = m });

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.TypeContract)) add("Type dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Exercice)) add("Exercice", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.NaturePrestation)) add("Nature prestation", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Objet)) add("Objet", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Fournisseur)) add("Fournisseur", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.CentreGestionnaire)) add("Centre Gestionnaire", "Obligatoire.");
        if (!r.DateDepense.HasValue) add("Date de Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.CentreBeneficiaire)) add("Centre bénéficiaire", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Devise)) add("Devise", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.DelaiExecution)) add("Délai d'exécution", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.ModePaiement)) add("Mode de paiement", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Validation)) add("Validation", "Obligatoire.");

        // Obligatoire si
        if (!string.IsNullOrWhiteSpace(r.ChargeSuiviAgent) &&
            string.IsNullOrWhiteSpace(r.ChargeSuiviMission))
        {
            add("Chargé de suivi-Mission", "Obligatoire si chargé de suivi.");
        }
        if (string.Equals(r.ModePaiement?.Trim(), "Virement", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(r.Banque)) add("Banque", "Obligatoire si Virement.");
            if (string.IsNullOrWhiteSpace(r.NumeroCompte)) add("N° compte", "Obligatoire si Virement.");
        }
        if (string.Equals(r.TypeContract?.Trim(), "Bon de Commande", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(r.PersonneHabilitee))
        {
            add("Personne Habilitée", "Obligatoire si Bon de Commande.");
        }
        if (string.Equals(r.Validation.Trim(), "Oui", StringComparison.OrdinalIgnoreCase) &&
            !r.DateValidation.HasValue)
        {
            add("Date de validation", "Obligatoire si Validation.");
        }

        // Format
        if (r.TauxRemise.HasValue && (r.TauxRemise < 0 || r.TauxRemise > 100)) add("Taux de remise", "Doit être entre 0 et 100.");
        if (r.TauxRetenueSource.HasValue && (r.TauxRetenueSource < 0 || r.TauxRetenueSource > 100)) add("Taux de retenue source", "Doit être entre 0 et 100.");
    }

    private static void ValidateBordereau(BordereauRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Borderaux de prix", RowNumber = r.ExcelRow, Column = c, Message = m });

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.DesignationProduit)) add("Désignation Produit", "Obligatoire.");
        if (!r.Quantite.HasValue) add("Quantité", "Obligatoire.");
        if (!r.PuMinimum.HasValue) add("PU minimum", "Obligatoire.");
        if (!r.PrixUnitaire.HasValue) add("Prix Unitaire", "Obligatoire.");

        // Format
        if (r.Quantite.HasValue && r.Quantite <= 0) add("Quantité", "Doit être > 0.");
        if (r.PuMinimum.HasValue && r.PuMinimum < 0) add("PU minimum", "Doit être >= 0.");
        if (r.PrixUnitaire.HasValue && r.PrixUnitaire < 0) add("Prix Unitaire", "Doit être >= 0.");
        if (r.Tva.HasValue && (r.Tva < 0 || r.Tva > 100)) add("Taux TVA", "Doit être entre 0 et 100.");
    }

    private static void ValidateOrdreService(OrdreServiceRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Ordre de service", RowNumber = r.ExcelRow, Column = c, Message = m });

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.NumeroOrdreService)) add("N° Ordre de service", "Obligatoire.");
        if (!r.DateOs.HasValue) add("Date OS", "Obligatoire.");
        if (!r.DateNotificationOs.HasValue) add("Date Notification OS", "Obligatoire.");
        if (!r.DateEffetOs.HasValue) add("Date d'effet OS", "Obligatoire.");
    }

    private static void ValidateEngagement(EngagementRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Engagement", RowNumber = r.ExcelRow, Column = c, Message = m });

        bool estNouvelEngagement = !string.IsNullOrWhiteSpace(r.CategorieBudget) && r.DateEngagement.HasValue;

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.CodeLigneBudgetaire)) add("Code Ligne Budgétaire", "Obligatoire.");
        if (!r.MontantEngagerCP.HasValue) add("Montant Engagé CP", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Validation)) add("Validation", "Obligatoire.");
        if (!estNouvelEngagement)
        {
            if (!string.IsNullOrWhiteSpace(r.CategorieBudget) && !r.DateEngagement.HasValue)
                add("Date Engagement", "Obligatoire si la catégorie budget est renseignée.");
            if (string.IsNullOrWhiteSpace(r.CategorieBudget) && r.DateEngagement.HasValue)
                add("Catégorie Budget", "Obligatoire si la date d'engagement est renseignée.");
        }

        // Obligatoire si
        if (string.Equals(r.Validation?.Trim(), "Oui", StringComparison.OrdinalIgnoreCase) &&
            !r.DateValidation.HasValue && estNouvelEngagement)
        {
            add("Date de validation", "Obligatoire si Validation.");
        }

        // Format
        if (r.MontantEngagerCP.HasValue && r.MontantEngagerCP < 0) add("Montant Engagé CP", "Doit être >= 0.");
        if (r.MontantEngagerCE.HasValue && r.MontantEngagerCE < 0) add("Montant Engagé CE", "Doit être >= 0.");
    }

    private static void ValidateLivraison(LivraisonRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Livraison", RowNumber = r.ExcelRow, Column = c, Message = m });

        bool estNouvelleLivraison = !string.IsNullOrWhiteSpace(r.NumeroLivraison) && r.DateLivraison.HasValue;

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.ProduitNPrix)) add("N° Livraison", "Obligatoire.");
        if (!r.ProduitQuantiteLivree.HasValue) add("Produit-Quantité Livrée", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Validation)) add("Validation", "Obligatoire.");
        if (!estNouvelleLivraison)
        {
            if (!string.IsNullOrWhiteSpace(r.NumeroLivraison) && !r.DateLivraison.HasValue)
                add("Date Livraison", "Obligatoire si le BL est renseigné.");
            if (string.IsNullOrWhiteSpace(r.NumeroLivraison) && r.DateLivraison.HasValue)
                add("N° BL", "Obligatoire si la date de livraison est renseignée.");
        }

        // Obligatoire si
        if (string.Equals(r.Validation?.Trim(), "Oui", StringComparison.OrdinalIgnoreCase) &&
            !r.DateValidation.HasValue && estNouvelleLivraison)
        {
            add("Date de validation", "Obligatoire si Validation.");
        }

        // Format
        if (r.ProduitQuantiteLivree.HasValue && r.ProduitQuantiteLivree <= 0) add("Produit-Quantité Livrée", "Doit être > 0.");
        if (r.ProduitQuantiteAcceptee.HasValue && r.ProduitQuantiteAcceptee < 0) add("Produit-Quantité Acceptée", "Doit être >= 0.");
    }

    private static void ValidateReception(ReceptionRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Réception", RowNumber = r.ExcelRow, Column = c, Message = m });

        bool estNouvelleReception = !string.IsNullOrWhiteSpace(r.NatureReception) 
            && !string.IsNullOrWhiteSpace(r.TypeReception) && r.DateReception.HasValue;

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.NumeroLivraison)) add("N° Livraison", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Validation)) add("Validation", "Obligatoire.");
        if (!estNouvelleReception)
        {
            if (string.IsNullOrWhiteSpace(r.TypeReception) && (!string.IsNullOrWhiteSpace(r.NatureReception) || r.DateReception.HasValue))
                add("Type Réception", "Obligatoire si Nature Réception et/ou Date Réception sont renseignées.");
            if (string.IsNullOrWhiteSpace(r.NatureReception) && (!string.IsNullOrWhiteSpace(r.TypeReception) || r.DateReception.HasValue))
                add("Nature Réception", "Obligatoire si Type Réception et/ou Date Réception sont renseignées.");
            if (!r.DateReception.HasValue && !string.IsNullOrWhiteSpace(r.TypeReception) && !string.IsNullOrWhiteSpace(r.NatureReception))
                add("Date Réception", "Obligatoire si Nature Réception et/ou Type Réception sont renseignées.");
        }

        // Obligatoire si
        if (string.Equals(r.Validation?.Trim(), "Oui", StringComparison.OrdinalIgnoreCase) &&
            !r.DateValidation.HasValue && estNouvelleReception)
        {
            add("Date de validation", "Obligatoire si Validation.");
        }
    }

    private static void ValidateFacture(FactureRowDto r, List<ImportErrorDto> e)
    {
        void add(string c, string m) => e.Add(new() { Sheet = "Facture", RowNumber = r.ExcelRow, Column = c, Message = m });

        bool estNouvelleFacture = !string.IsNullOrWhiteSpace(r.NumeroFactureFournisseur)
            && r.DateFacture.HasValue && r.DateDepotFacture.HasValue;

        // Obligatoire
        if (string.IsNullOrWhiteSpace(r.NumeroDepense)) add("N° Dépense", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.NumeroLivraison)) add("N° Livraison", "Obligatoire.");
        if (string.IsNullOrWhiteSpace(r.Validation)) add("Validation", "Obligatoire.");
        if (!estNouvelleFacture)
        {
            if (string.IsNullOrWhiteSpace(r.NumeroFactureFournisseur) && (r.DateFacture.HasValue || r.DateDepotFacture.HasValue))
                add("N° Facture Fournisseur", "Obligatoire si Date Facture et/ou Date Dépôt Facture sont renseignées.");
            if (!r.DateFacture.HasValue && (!string.IsNullOrWhiteSpace(r.NumeroFactureFournisseur) || r.DateDepotFacture.HasValue))
                add("Date Facture", "Obligatoire si N° Facture Fournisseur et/ou Date Dépôt Facture sont renseignées.");
            if (!r.DateDepotFacture.HasValue && (!string.IsNullOrWhiteSpace(r.NumeroFactureFournisseur) || r.DateFacture.HasValue))
                add("Date Dépôt Facture", "Obligatoire si N° Facture Fournisseur et/ou Date Facture sont renseignées.");
        }

        // Obligatoire si
        if (string.Equals(r.Validation?.Trim(), "Oui", StringComparison.OrdinalIgnoreCase) &&
            !r.DateValidation.HasValue && estNouvelleFacture)
        {
            add("Date de validation", "Obligatoire si Validation.");
        }

        // Format
        if (r.MontantRevision.HasValue && r.MontantRevision < 0) add("Montant Révision", "Doit être >= 0.");
        if (r.TauxRsSurTvaApplique.HasValue && (r.TauxRsSurTvaApplique < 0 || r.TauxRsSurTvaApplique > 100)) add("Taux RS sur TVA appliqué", "Doit être entre 0 et 100.");
        if (r.PenaliteSpecialeNbreJoursManquement.HasValue && r.PenaliteSpecialeNbreJoursManquement < 0) add("Pénalité spéciale - Nbre jours manquement", "Doit être >= 0.");
        if (r.PenaliteSpecialeIndemnisation.HasValue && r.PenaliteSpecialeIndemnisation < 0) add("Pénalité spéciale - Indemnisation", "Doit être >= 0.");
    }

    private static void ValidateCrossSheets(WorkbookImportDto wb, List<ImportErrorDto> errors)
    {
        var setDepense = wb.FicheGenerale.Select(x => x.NumeroDepense).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var setProduit = wb.Bordereaux.Select(x => x.DesignationProduit).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var setLivraison = wb.Livraisons.Select(x => x.NumeroLivraison).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var b in wb.Bordereaux)
            if (!setDepense.Contains(b.NumeroDepense))
                errors.Add(new() { Sheet = "Borderaux de prix", RowNumber = b.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });

        foreach (var os in wb.OrdresService)
            if (!setDepense.Contains(os.NumeroDepense))
                errors.Add(new() { Sheet = "Ordre de service", RowNumber = os.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });

        foreach (var eng in wb.Engagements)
            if (!setDepense.Contains(eng.NumeroDepense))
                errors.Add(new() { Sheet = "Engagement", RowNumber = eng.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });

        foreach (var liv in wb.Livraisons)
        {
            if (!setDepense.Contains(liv.NumeroDepense))
                errors.Add(new() { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });
            if (!setProduit.Contains(liv.ProduitNPrix))
                errors.Add(new() { Sheet = "Livraison", RowNumber = liv.ExcelRow, Column = "Produit-N° Prix", Message = "Non trouvé en Borderaux de prix." });
        }
            
        foreach (var rec in wb.Receptions)
        {
            if (!setDepense.Contains(rec.NumeroDepense))
                errors.Add(new() { Sheet = "Réception", RowNumber = rec.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });
            if (!setLivraison.Contains(rec.NumeroLivraison))
                errors.Add(new() { Sheet = "Réception", RowNumber = rec.ExcelRow, Column = "N° Livraison", Message = "Non trouvé en Livraison." });
        }

        foreach (var fac in wb.Factures)
        {
            if (!setDepense.Contains(fac.NumeroDepense))
                errors.Add(new() { Sheet = "Facture", RowNumber = fac.ExcelRow, Column = "N° Dépense", Message = "Non trouvé en Fiche Générale." });
            if (!setLivraison.Contains(fac.NumeroLivraison))
                errors.Add(new() { Sheet = "Facture", RowNumber = fac.ExcelRow, Column = "N° Livraison", Message = "Non trouvé en Livraison." });
        }
    }

    private static void ValidateAll(WorkbookImportDto wb, List<ImportErrorDto> errors)
    {
        foreach (var fg in wb.FicheGenerale) ValidateFiche(fg, errors);
        foreach (var br in wb.Bordereaux) ValidateBordereau(br, errors);
        foreach (var os in wb.OrdresService) ValidateOrdreService(os, errors);
        foreach (var eng in wb.Engagements) ValidateEngagement(eng, errors);
        foreach (var liv in wb.Livraisons) ValidateLivraison(liv, errors);
        foreach (var rec in wb.Receptions) ValidateReception(rec, errors);
        foreach (var fac in wb.Factures) ValidateFacture(fac, errors);
        ValidateCrossSheets(wb, errors);
    }

    //HELPERS
    //CODE_FILS
    private async Task<int> GetMaxCodeFilsAsync(CancellationToken ct)
    {
        var max = await _db.DepMarchesH
            .Where(x => x.CodeFils != null && x.CodeFils.Length == 8)
            .Select(x => x.CodeFils)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(max) || !int.TryParse(max, out var maxInt))
            return 0;

        return maxInt;
    }

    //[CODE_H] depense
    private async Task<int> GetMaxCodeHAsync(CancellationToken ct)
    {
        var max = await _db.DepMarchesH
            .Where(x => x.Niv == 2 && x.CodeH != null)
            .Select(x => x.CodeH!)
            .ToListAsync(ct);

        var maxVal = 0;
        foreach (var ch in max)
        {
            var parts = ch.Split('.');
            if (parts.Length > 1 && int.TryParse(parts[1], out var i) && i > maxVal)
                maxVal = i;
        }

        return maxVal;
    }

    //NUM_ORDRE(engagement)
    private async Task<int> GetMaxNumOrdreAsync(CancellationToken ct)
    {
        // Récupérer tous les numéros qui commencent par "ENG/" pour ce marché
        var numeros = await _db.CptEngagements
            .Where(x => x.NumeroOrdre != null)
            .Select(x => x.NumeroOrdre)
            .ToListAsync(ct);

        var maxNum = 0;

        foreach (var numero in numeros)
        {
            // Format attendu : "ENG/147/2026"
            var parts = numero.Split('/');

            // Vérifier qu'on a bien 3 parties et que la 2ème est un nombre
            if (parts.Length == 3
                && parts[0] == "ENG"
                && int.TryParse(parts[1], out var num)
                && num > maxNum)
            {
                maxNum = num;
            }
        }

        return maxNum;
    }
    public async Task<string> GenererProchainNumOrdreAsync(CancellationToken ct)
    {
        var anneeCourante = DateTime.Now.Year;
        var dernierNumero = await GetMaxNumOrdreAsync(ct);
        var prochainNumero = dernierNumero + 1;

        return $"ENG/{prochainNumero}/{anneeCourante}";
    }

    // Code Avancement
    private async Task<decimal> GetMaxCodeAvancementAsync(CancellationToken ct)
    {
        var max = await _db.DepAvancementFactures
            .Select(x => x.CodeAvancement)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(ct);

        return max;
    }

    private static ImportResultDto Fail(string sheet, int row, string col, string msg) =>
        new() { Success = false, TotalRows = 0, InsertedRows = 0, Errors = new() { new() { Sheet = sheet, RowNumber = row, Column = col, Message = msg } } };

    private static void AddDbErr(List<DatabaseErrorDto> dbErrors, string message, string errorType = "", string sqlErrorCode = "", string entityName = "")
    => dbErrors.Add(new DatabaseErrorDto
    {
        Sheet = "Database",
        RowNumber = 0,
        Column = "-",
        Message = message,
        ErrorType = errorType,
        SqlErrorCode = sqlErrorCode,
        EntityName = entityName,
        Timestamp = DateTime.UtcNow
    });
}