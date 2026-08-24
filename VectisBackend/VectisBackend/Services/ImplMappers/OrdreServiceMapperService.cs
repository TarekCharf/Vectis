using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public class OrdreServiceMapperService : IOrdreServiceMapperService
    {
        public DepOrdreService MapToDepOrdreService(OrdreServiceRowDto row, decimal codeMarche, int numOS)
        {
            return new DepOrdreService
            {
                NumeroOrdreOrdreService = numOS,
                NumeroOrdreService = row.NumeroOrdreService,
                CodeMarche = codeMarche,
                CodeTypeOrdreService = 1,
                DateOrdreService = row.DateOs,
                DateNotifOrdreService = row.DateNotificationOs,
                DateEffetOrdreService = row.DateEffetOs,
                OrdreNotification = 1,
                CodeGroupement = "-1",
                CodeMotifArret = 1,
                Valide = 1
            };
        }
    }
}
