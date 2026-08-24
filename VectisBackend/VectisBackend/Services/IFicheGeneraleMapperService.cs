using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services
{
    public interface IFicheGeneraleMapperService
    {
        DepMarche MapFicheGeneraleToDepMarche(FicheGeneraleRowDto row, decimal codeTypeContrat, 
       decimal codeFournisseur, decimal codeDevise, decimal? codeTypeBudget, int? codeTypePrestation, decimal? codeNaturePrestation, 
       decimal? codeCentreGes, decimal? codeMo, decimal? codeModePaiement, decimal? codeBanquePaiement, decimal? personneHabilitee,
       decimal? delaiExecution, decimal? delaiGarantie, decimal? uniteDelaiExecution, decimal? uniteDelaiGarantie);

        DepMarcheCentre MapToDepMarcheCentre(decimal codeMarche, decimal codeCentreBeneficiaire);

        DepChargeSuivi? MapToDepChargeSuivi(FicheGeneraleRowDto row, decimal codeMarche, decimal? codeAgentChargeSuivi);

        DepMarchesH MapDepenseNodeToDepMarchesH(
            string codeFils, string libelleNumeroDepense, string codePere, string codeH,
            decimal nbProduits, string typeDepense, decimal codeMarche);
    }
}
