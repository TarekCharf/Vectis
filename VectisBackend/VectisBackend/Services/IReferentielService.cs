using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Services;

public record FicheGeneraleCodes(
    decimal CodeTypeContract,
    decimal CodeFournisseur,
    decimal CodeDevise,
    decimal CodeCentreBeneficiaire,
    decimal? CodeTypeBudget,
    int? CodeTypePrestation,
    decimal? CodeNaturePrestation,
    decimal? CodeCentreGestionnaire,
    decimal? CodeMaitreOuvrage,
    decimal? CodeModePaiement,
    decimal? CodeBanquePaiement,
    decimal? CodePersonneHabilitee,
    decimal? CodeAgentChargeSuivi,
    decimal? DelaiExecution,
    decimal? DelaiGarantie,
    decimal? UniteDelaiExecution,
    decimal? UniteDelaiGarantie
);

public record EngagementCodes(
    decimal CodeTypeCredit, 
    decimal Annee, 
    decimal CodeTypeContract,
    decimal CodeDepense, 
    decimal CodeLigne,
    decimal DisponibleCP, 
    double DisponibleCE,
    double CreditPreEngCP, 
    double CreditPreEngCE,
    double CreditEngCP, 
    double CreditEngCE
);

public record LivraisonCodes(
    decimal CodeDepense,
    decimal? CodeResponsable,
    decimal? CodeMagasin,
    decimal? CodeMagasinier,
    string CodeFilsProduit,
    decimal TauxTva,
    decimal Quantite,
    decimal CodeCentreResp
    );

public record ReceptionCodes(
    decimal CodeDepense,
    decimal? CodeNature,
    decimal? CodeType,
    decimal? CodeReleve
);

public record ProduitFactureItem(
    decimal? CodeProduit,
    string? CodeFilsProduit,
    decimal? QuantiteLivreeProduit,
    decimal? QuantiteAcepteeProduit,
    decimal? PrixUnite,
    decimal? TauxTvaProduit,
    string? RemiseProduit,
    decimal? CodeCentreResp
);
public record FactureCodes(
    decimal CodeDepense,
    decimal CodeReleve,
    decimal? Periode,
    decimal CodeFournisseur,
    decimal? MontantFactureHT,
    decimal? MontantFactureTTC,
    List<ProduitFactureItem> Produits,
    decimal? TauxTvaFacture,
    decimal? TauxRemiseFacture,
    decimal? TauxRsFacture
);

public interface IReferentielService
{
    Task<FicheGeneraleCodes> ResolveFicheGeneraleCodesAsync(FicheGeneraleRowDto row, CancellationToken ct, List<ImportErrorDto> errors);
    Task<(decimal codeProduit, string produitId, string designation, string? unite)?> ResolveProduitCodeAsync(string designationProduit, CancellationToken ct);
    Task<EngagementCodes> ResolveEngagementCodesAsync(EngagementRowDto row, CancellationToken ct, List<ImportErrorDto> errors);
    Task<LivraisonCodes> ResolveLivraisonCodesAsync(LivraisonRowDto row, CancellationToken ct, List<ImportErrorDto> errors);
    Task<ReceptionCodes> ResolveReceptionCodesAsync(ReceptionRowDto row, CancellationToken ct, List<ImportErrorDto> errors);
    Task<FactureCodes> ResolveFactureCodesAsync(FactureRowDto row, CancellationToken ct, List<ImportErrorDto> errors);
}