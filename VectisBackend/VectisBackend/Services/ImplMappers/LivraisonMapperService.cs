using Microsoft.Extensions.Options;
using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public class LivraisonMapperService : ILivraisonMapperService
    {
        public DepReleveMarche MapToDepReleveMarche(LivraisonRowDto row, decimal NumOrdreReleve, string NumeroReleve, decimal CodeMarche, decimal? CodeResponsable, decimal? CodeMagasin, decimal? CodeMagasinier)
        {
            return new DepReleveMarche
            {
                NumeroOrdreReleve = NumOrdreReleve,
                NumeroReleve = NumeroReleve,
                CodeMarche = CodeMarche,
                ReferenceLivraison = row.NumeroLivraison,
                CodeResponsable = CodeResponsable,
                CodeMagasin = CodeMagasin,
                CodeMagasinier = CodeMagasinier,
                DateReleve = row.DateLivraison,
                DateBonLivraison = row.DateLivraison,
                HeureLivraison = row.HeureLivraison ?? "",
                DateRelevePrevis = row.DateLivraisonPrevisionnelle ?? row.DateLivraison,
                DateDepot = row.DateDepot ?? row.DateLivraison,
                ReleveValide = ParseOuiNonNumeric(row.Validation),
                DateValidation = row.DateValidation,
                EtabliParMo = 0,
                MtReleveTtc = 0, // This will be calculated later
                MtReleveHt = 0, // This will be calculated later
                IsReceptionned = 0, // Default value
                IsFactured = 0 // Default value
            };
        }
        public DepLigneReleveMarche MapToDepLigneReleveMarche(LivraisonRowDto row, decimal CodeReleve, string CodeFilsProduit, decimal TauxTva, decimal Quantite)
        {
            return new DepLigneReleveMarche
            {
                CodeReleve = CodeReleve,
                CodeFilsProduit = CodeFilsProduit,
                TauxTva = TauxTva,
                PourcLivraison = (double?)((row.ProduitQuantiteLivree / Quantite) * 100), // Calculate percentage of delivery
                QteExecution = row.ProduitQuantiteLivree,
                QteLiv = row.ProduitQuantiteLivree,
                QteAjournee = 0,
                TauxRefaction = 0,
                QteRefaction = 0,
                Controle = 0,
                QteRetourne = 0
            };
        }

        public DepVentilQuantiteReleve MapToDepVentilQuantiteReleve(LivraisonRowDto row, decimal CodeCentreResp, decimal CodeReleve, string CodeFilsProduit)
        {
            return new DepVentilQuantiteReleve
            {
                CodeCentre = CodeCentreResp,
                CodeReleve = CodeReleve,
                CodeFilsProduit = CodeFilsProduit,
                QuantiteLiv = row.ProduitQuantiteLivree,
                QuantiteAcpt = row.ProduitQuantiteAcceptee ?? 0
            };
        }

        private static decimal? ParseOuiNonNumeric(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            var v = value.Trim().ToUpperInvariant();
            if (v == "OUI") return 1;
            if (v == "NON") return 0;
            return null;
        }
    }
}
