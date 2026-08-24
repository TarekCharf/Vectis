using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public class BorderauxMapperService : IBorderauxMapperService
    {
        public DepMarchesH MapProduitNodeToDepMarchesH(
        BordereauRowDto row, string codeFils, string codePere, string codeH,
        decimal referenceProduit, string? uniteProduit, string refUtil, string refUtilFormat)
        {
            var qte = row.Quantite ?? 0m;
            var puMin = row.PuMinimum ?? 0m;
            var pu = row.PrixUnitaire ?? 0m;
            var tva = row.Tva ?? 0m;
            var prixTtc = (pu + (pu * tva / 100m)) * qte;

            return new DepMarchesH
            {
                RefUtil = refUtil,
                CodeFils = codeFils,
                Libelle = row.DesignationProduit,
                CodePere = codePere,        // code_fils parent (dépense)
                Type = "Produit",
                Selection = 0,
                Niv = 3,
                CodeH = codeH,              // parentCodeH + ".1", ".2", ...
                Quantite = qte,
                PrixMin = puMin,
                PrixUnit = pu,
                Unite = uniteProduit,
                RefUtilFormat = refUtilFormat,
                ReferenceProduit = referenceProduit,
                TauxTva = tva,
                PrixTtc = prixTtc,
                CaractLigne = "Caractéristiques techniques",
                RefTri = refUtilFormat,
                Hddtva = 0,
                CalculFraisApproche = null,
                QuantiteMax = 0,
                Remise = ParseOuiNonChar(row.Remise),                  // 1/0
                PrixRevisable = ParseOuiNonNumeric(row.PrixRevisable), // 1/0
                AppliqueRs = ParseOuiNonNumeric(row.Rs),               // 1/0
                AppliqueRsTva = ParseOuiNonNumeric(row.RsSurTva)       // 1/0
            };
        }

        public GenTarifFournisseur MapToGenTarifFournisseur(
            BordereauRowDto bordereauRow, decimal tauxRemise, decimal codeFournisseur, decimal codeProduit)
        {
            return new GenTarifFournisseur
            {
                CodeFrs = codeFournisseur,
                CodeProduit = codeProduit,
                PrixFrs = bordereauRow.PrixUnitaire ?? 0m,
                TauxRemise = tauxRemise
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

        private static string? ParseOuiNonChar(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "0";
            var v = value.Trim().ToUpperInvariant();
            if (v == "OUI") return "1";
            if (v == "NON") return "0";
            return null;
        }
    }
}
