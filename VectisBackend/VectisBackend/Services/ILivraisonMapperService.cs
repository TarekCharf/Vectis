using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services
{
    public interface ILivraisonMapperService
    {
        DepReleveMarche MapToDepReleveMarche(
            LivraisonRowDto row, decimal NumOrdreReleve, string NumeroReleve, decimal CodeMarche, 
            decimal? CodeResponsable, decimal? CodeMagasin, decimal? CodeMagasinier);

        DepLigneReleveMarche MapToDepLigneReleveMarche(
            LivraisonRowDto row, decimal CodeReleve, string CodeFilsProduit, decimal TauxTva, decimal Quantite);

        DepVentilQuantiteReleve MapToDepVentilQuantiteReleve(
            LivraisonRowDto row, decimal CodeCentreResp, decimal CodeReleve, string CodeFilsProduit);
    }
}
