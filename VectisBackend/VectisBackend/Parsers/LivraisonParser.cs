using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class LivraisonParser : Parser
{
    public override string SheetName => "Livraison";

    public override void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors)
    {
        if (!ctx.IncludeLivraison)
            return;

        var ws = workbook.Worksheets.FirstOrDefault(x => x.Name == SheetName);
        if (ws == null)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "Feuille introuvable." });
            return;
        }

        var last = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= last; r++)
        {
            var row = new LivraisonRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                NumeroLivraison = ws.Cell(r, 2).GetString().Trim(),
                ResponsableLivraison = ws.Cell(r, 3).GetString().Trim(),
                Magasin = ws.Cell(r, 4).GetString().Trim(),
                Magasinier = ws.Cell(r, 5).GetString().Trim(),
                DateLivraison = TryDate(ws.Cell(r, 6), SheetName, r, "Date livraison", errors),
                HeureLivraison = TryTime(ws.Cell(r, 7), SheetName, r, "Heure livraison", errors),
                DateLivraisonPrevisionnelle = TryDate(ws.Cell(r, 8), SheetName, r, "Date livraison prévisionnelle", errors),
                DateDepot = TryDate(ws.Cell(r, 9), SheetName, r, "Date dépôt", errors),
                ProduitNPrix = ws.Cell(r, 10).GetString().Trim(),
                ProduitQuantiteLivree = TryDecimal(ws.Cell(r, 11), SheetName, r, "Produit-Quantité Livrée", errors),
                ProduitQuantiteAcceptee = TryDecimal(ws.Cell(r, 12), SheetName, r, "Produit-Quantité acceptée", errors),
                Validation = ws.Cell(r, 13).GetString().Trim(),
                DateValidation = TryDate(ws.Cell(r, 14), SheetName, r, "Date de validation", errors)
            };

            // Ignore la ligne si les champs d'en-tête principaux sont vides
            if (string.IsNullOrWhiteSpace(row.NumeroDepense) && 
                string.IsNullOrWhiteSpace(row.NumeroLivraison) &&
                !row.DateLivraison.HasValue &&
                string.IsNullOrWhiteSpace(row.ProduitNPrix) &&
                !row.ProduitQuantiteLivree.HasValue)
                continue;

            data.Livraisons.Add(row);
        }

        AddDuplicateErrors(data.Livraisons, r => r.NumeroLivraison, r => r.ExcelRow, SheetName, "N° Livraison", errors);

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent) alors qu'elle a été demandée à l'import." });
        }
        else if (data.Livraisons.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes — vérifiez que le fichier a bien été rempli." });
        }
    }
}