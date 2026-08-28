using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public class OrdreServiceMapperService : IOrdreServiceMapperService
    {
        public DepOrdreService MapToDepOrdreService(OrdreServiceRowDto row, decimal codeMarche, int numOS, decimal codeTypeContrat, DateTime dateDepense)
        {
            if (codeTypeContrat == 2)
            {
                return new DepOrdreService
                {
                    NumeroOrdreOrdreService = numOS,
                    NumeroOrdreService = "1",
                    CodeMarche = codeMarche,
                    CodeTypeOrdreService = 9,
                    DateOrdreService = dateDepense,
                    DateNotifOrdreService = dateDepense,
                    DateEffetOrdreService = dateDepense,
                    OrdreNotification = 1,
                    CodeGroupement = "-1",
                    CodeMotifArret = 1,
                    Valide = 1
                };
            }
            return new DepOrdreService
            {
                NumeroOrdreOrdreService = numOS,
                NumeroOrdreService = row.NumeroOrdreService,
                CodeMarche = codeMarche,
                CodeTypeOrdreService = 9,
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
