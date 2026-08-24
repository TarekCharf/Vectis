using ClosedXML.Excel;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class OrdreServiceParser : Parser
{
    public override string SheetName => "Ordre de service";

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
            var row = new OrdreServiceRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                NumeroOrdreService = ws.Cell(r, 2).GetString().Trim(),
                DateOs = TryDate(ws.Cell(r, 3), SheetName, r, "Date OS", errors),
                DateNotificationOs = TryDate(ws.Cell(r, 4), SheetName, r, "Date Notification OS", errors),
                DateEffetOs = TryDate(ws.Cell(r, 5), SheetName, r, "Date d'effet OS", errors)
            };

            if (string.IsNullOrWhiteSpace(row.NumeroDepense) && 
                string.IsNullOrWhiteSpace(row.NumeroOrdreService) &&
                !row.DateOs.HasValue &&
                !row.DateNotificationOs.HasValue &&
                !row.DateEffetOs.HasValue)
                continue;

            data.OrdresService.Add(row);
        }

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent)." });
        }
        else if (data.OrdresService.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes (N° Dépense et N° Ordre de service tous vides) — vérifiez que le fichier a bien été rempli." });
        }
    }
}