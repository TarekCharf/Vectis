using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class FactureParser : Parser
{
    public override string SheetName => "Facture";

    public override void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors)
    {
        if (!ctx.IncludeFacture)
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
            var row = new FactureRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                NumeroFactureFournisseur = ws.Cell(r, 2).GetString().Trim(),
                DateFacture = TryDate(ws.Cell(r, 3), SheetName, r, "Date facture", errors),
                DateDepotFacture = TryDate(ws.Cell(r, 4), SheetName, r, "Date dépôt facture", errors),
                Periode = ws.Cell(r, 5).GetString().Trim(),
                MontantRevision = TryDecimal(ws.Cell(r, 6), SheetName, r, "Montant Révision", errors),
                CalculerRgTotal = ws.Cell(r, 7).GetString().Trim(),
                CalculerPenalitesRetard = ws.Cell(r, 8).GetString().Trim(),
                PourcentageReception = ws.Cell(r, 9).GetString().Trim(),
                PourcentageMarche = ws.Cell(r, 10).GetString().Trim(),
                TauxRsSurTvaApplique = TryDecimal(ws.Cell(r, 11), SheetName, r, "Taux RS sur TVA Appliqué", errors),
                PenaliteSpeciale = ws.Cell(r, 12).GetString().Trim(),
                PenaliteSpecialeNbreJoursManquement = TryDecimal(ws.Cell(r, 13), SheetName, r, "Pénalité Spéciale-Nombre de jour de manquement", errors),
                PenaliteSpecialeIndemnisation = TryDecimal(ws.Cell(r, 14), SheetName, r, "Pénalité Spéciale-Indemnisation", errors),
                Observation = ws.Cell(r, 15).GetString().Trim(),
                Justification = ws.Cell(r, 16).GetString().Trim(),
                NumeroLivraison = ws.Cell(r, 17).GetString().Trim(),
                Validation = ws.Cell(r, 18).GetString().Trim(),
                DateValidation = TryDate(ws.Cell(r, 19), SheetName, r, "Date de validation", errors)
            };

            // Ignore la ligne si les identifiants clés sont vides
            if (string.IsNullOrWhiteSpace(row.NumeroDepense) && 
                string.IsNullOrWhiteSpace(row.NumeroFactureFournisseur) &&
                !row.DateFacture.HasValue &&
                !row.DateDepotFacture.HasValue &&
                string.IsNullOrWhiteSpace(row.NumeroLivraison))
                continue;

            data.Factures.Add(row);
        }

        AddDuplicateErrors(data.Factures, r => r.NumeroLivraison, r => r.ExcelRow, SheetName, "N° Livraison", errors);

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent) alors qu'elle a été demandée à l'import." });
        }
        else if (data.Factures.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes — vérifiez que le fichier a bien été rempli." });
        }
    }
}