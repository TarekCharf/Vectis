using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class ReceptionParser : Parser
{
    public override string SheetName => "Réception";

    public override void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors)
    {
        if (!ctx.IncludeReception)
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
            var row = new ReceptionRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                DateReception = TryDate(ws.Cell(r, 2), SheetName, r, "Date réception", errors),
                NatureReception = ws.Cell(r, 3).GetString().Trim(),
                TypeReception = ws.Cell(r, 4).GetString().Trim(),
                Lieux = ws.Cell(r, 5).GetString().Trim(),
                NumFicheReception = ws.Cell(r, 6).GetString().Trim(),
                HeureReception = TryTime(ws.Cell(r, 7), SheetName, r, "Heure réception", errors),
                NumeroLivraison = ws.Cell(r, 8).GetString().Trim(),
                Validation = ws.Cell(r, 9).GetString().Trim(),
                DateValidation = TryDate(ws.Cell(r, 10), SheetName, r, "Date de validation", errors)
            };

            // Ignore la ligne si les champs d'en-tête principaux sont vides
            if (string.IsNullOrWhiteSpace(row.NumeroDepense) &&
                !row.DateReception.HasValue &&
                string.IsNullOrWhiteSpace(row.NatureReception) &&
                string.IsNullOrWhiteSpace(row.TypeReception) &&
                string.IsNullOrWhiteSpace(row.NumeroLivraison))
                continue;

            data.Receptions.Add(row);
        }

        AddDuplicateErrors(data.Receptions, r => r.NumeroLivraison, r => r.ExcelRow, SheetName, "N° Livraison", errors);

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent) alors qu'elle a été demandée à l'import." });
        }
        else if (data.Receptions.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes — vérifiez que le fichier a bien été rempli." });
        }
    }
}