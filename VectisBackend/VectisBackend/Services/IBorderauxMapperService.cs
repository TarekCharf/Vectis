using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services
{
    public interface IBorderauxMapperService
    {
        DepMarchesH MapProduitNodeToDepMarchesH(
       BordereauRowDto row, string codeFils, string codePere, string codeH,
       decimal referenceProduit, string? uniteProduit, string refUtil, string refUtilFormat);

        GenTarifFournisseur MapToGenTarifFournisseur(
            BordereauRowDto bordereauRow, decimal tauxRemise, decimal codeFournisseur, decimal codeProduit);
    }
}
