using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public interface IFactureMapperService
    {
        DepFacture MapToDepFacture(
            FactureRowDto row,
            decimal numOrdreFacture,
            string numeroFacture,
            decimal codeMarche,
            decimal? periode,
            decimal? codeFournisseur,
            decimal? montantHT,
            decimal? montantTTC);

        DepFactureReleve MapToDepFactureReleve(decimal codeFacture, decimal codeReleve);

        DepAvancementFacture MapToDepAvancementFacture(
            decimal codeAvancement,
            decimal codeFacture,
            string codeFilsProduit,
            decimal quantiteLivre,
            decimal quantiteAcceptee);

        DepVentilMntsFacture MapToDepVentilMntsFacture(
            FactureRowDto row,
            decimal codeFacture,
            decimal codeProduit,
            decimal remiseProduit,
            decimal montantProduitHT,
            string codeFilsProduit,
            decimal? codeCentre,
            decimal? tauxRs,
            decimal tauxTva);

        DepMontantsFacture MapToDepMontantsFacture(FactureRowDto row, decimal codeFacture);

        DepAllMontantsFacture MapToDepAllMontantsFacture(FactureRowDto row, decimal codeFacture);
    }
}