using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class EngagementParser : Parser
{
    public override string SheetName => "Engagement";

    public override void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors)
    {
        if (!ctx.IncludeEngagement)
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
            var row = new EngagementRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                CategorieBudget = ws.Cell(r, 2).GetString().Trim(),
                DateEngagement = TryDate(ws.Cell(r, 3), SheetName, r, "Date engagement", errors),
                CodeLigneBudgetaire = ws.Cell(r, 4).GetString().Trim(),
                MontantEngagerCP = TryDecimal(ws.Cell(r, 5), SheetName, r, "Montant à engager CP", errors),
                MontantEngagerCE = TryDecimal(ws.Cell(r, 6), SheetName, r, "Montant à engager CE", errors),
                Validation = ws.Cell(r, 7).GetString().Trim(),
                DateValidation = TryDate(ws.Cell(r, 8), SheetName, r, "Date de validation", errors)
            };

            if (string.IsNullOrWhiteSpace(row.NumeroDepense) && 
                string.IsNullOrWhiteSpace(row.CategorieBudget) &&
                !row.DateEngagement.HasValue &&
                string.IsNullOrWhiteSpace(row.CodeLigneBudgetaire) &&
                !row.MontantEngagerCP.HasValue)
                continue;

            data.Engagements.Add(row);
        }

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent) alors qu'elle a été demandée à l'import." });
        }
        else if (data.Engagements.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes — vérifiez que le fichier a bien été rempli." });
        }
    }
}