using ClosedXML.Excel;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class BordereauxParser : Parser
{
    public override string SheetName => "Borderaux de prix";

    public override void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors)
    {
        var ws = workbook.Worksheets.FirstOrDefault(x => x.Name == SheetName);
        if (ws == null)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "Feuille introuvable." });
            return;
        }

        var last = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= last; r++)
        {
            var row = new BordereauRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                DesignationProduit = ws.Cell(r, 2).GetString().Trim(),
                Quantite = TryDecimal(ws.Cell(r, 3), SheetName, r, "Quantité", errors),
                PuMinimum = TryDecimal(ws.Cell(r, 4), SheetName, r, "PU minimum", errors),
                PrixUnitaire = TryDecimal(ws.Cell(r, 5), SheetName, r, "Prix Unitaire", errors),
                Tva = TryDecimal(ws.Cell(r, 6), SheetName, r, "TVA", errors),
                Remise = ws.Cell(r, 7).GetString().Trim(),
                PrixRevisable = ws.Cell(r, 8).GetString().Trim(),
                Rs = ws.Cell(r, 9).GetString().Trim(),
                RsSurTva = ws.Cell(r, 10).GetString().Trim()
            };

            if (string.IsNullOrWhiteSpace(row.NumeroDepense) && 
                string.IsNullOrWhiteSpace(row.DesignationProduit) &&
                !row.Quantite.HasValue &&
                !row.PuMinimum.HasValue &&
                !row.PrixUnitaire.HasValue)
                continue;

            data.Bordereaux.Add(row);
        }

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent)." });
        }
        else if (data.Bordereaux.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes (N° Dépense et Désignation Produit tous vides) — vérifiez que le fichier a bien été rempli." });
        }
    }
}