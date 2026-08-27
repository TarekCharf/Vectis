using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services
{
    public interface IOrdreServiceMapperService
    {
        DepOrdreService MapToDepOrdreService(OrdreServiceRowDto row, decimal codeMarche, int numOS, decimal codeTypeContrat, DateTime dateDepense);
    }
}
