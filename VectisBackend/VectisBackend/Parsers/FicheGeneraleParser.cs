using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;
using VectisBackend.Models.Import;

namespace VectisBackend.Parsers;

public class FicheGeneraleParser : Parser
{
    public override string SheetName => "Fiche Générale";

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
            var row = new FicheGeneraleRowDto
            {
                ExcelRow = r,
                NumeroDepense = ws.Cell(r, 1).GetString().Trim(),
                TypeContract = ws.Cell(r, 2).GetString().Trim(),
                Exercice = ws.Cell(r, 3).GetString().Trim(),
                TypePrestation = ws.Cell(r, 4).GetString().Trim(),
                NaturePrestation = ws.Cell(r, 5).GetString().Trim(),
                TypeBudget = ws.Cell(r, 6).GetString().Trim(),
                Objet = ws.Cell(r, 7).GetString().Trim(),
                Observation = ws.Cell(r, 8).GetString().Trim(),
                Fournisseur = ws.Cell(r, 9).GetString().Trim(),
                Representant = ws.Cell(r, 10).GetString().Trim(),
                CentreGestionnaire = ws.Cell(r, 11).GetString().Trim(),
                MaitreOuvrage = ws.Cell(r, 12).GetString().Trim(),
                DateDepense = TryDate(ws.Cell(r, 13), SheetName, r, "Date de Dépense", errors),
                CentreBeneficiaire = ws.Cell(r, 14).GetString().Trim(),
                Devise = ws.Cell(r, 15).GetString().Trim(),
                ChargeSuiviAgent = ws.Cell(r, 16).GetString().Trim(),
                ChargeSuiviMission = ws.Cell(r, 17).GetString().Trim(),
                DelaiExecution = TryDuration(ws.Cell(r, 18), SheetName, r, "Délai d'exécution", errors),
                DelaiGarantie = TryDuration(ws.Cell(r, 19), SheetName, r, "Délai de garantie", errors),
                ModePaiement = ws.Cell(r, 20).GetString().Trim(),
                Banque = ws.Cell(r, 21).GetString().Trim(),
                NumeroCompte = ws.Cell(r, 22).GetString().Trim(),
                PersonneHabilitee = ws.Cell(r, 23).GetString().Trim(),
                TauxRemise = TryFloat(ws.Cell(r, 24), SheetName, r, "Taux de remise", errors),
                TauxRetenueSource = TryFloat(ws.Cell(r, 25), SheetName, r, "Taux de retenue source", errors),
                Validation = ws.Cell(r, 26).GetString().Trim(),
                DateValidation = TryDate(ws.Cell(r, 27), SheetName, r, "Date de validation", errors)
            };

            if (string.IsNullOrWhiteSpace(row.NumeroDepense) &&
                string.IsNullOrWhiteSpace(row.TypeContract) &&
                string.IsNullOrWhiteSpace(row.Exercice) &&
                string.IsNullOrWhiteSpace(row.NaturePrestation) &&
                string.IsNullOrWhiteSpace(row.Objet) &&
                string.IsNullOrWhiteSpace(row.Fournisseur) &&
                string.IsNullOrWhiteSpace(row.CentreGestionnaire) &&
                !row.DateDepense.HasValue &&
                string.IsNullOrWhiteSpace(row.CentreBeneficiaire) &&
                string.IsNullOrWhiteSpace(row.Devise) &&
                string.IsNullOrWhiteSpace(row.DelaiExecution) &&
                string.IsNullOrWhiteSpace(row.ModePaiement))
                continue;

            data.FicheGenerale.Add(row);
        }

        AddDuplicateErrors(data.FicheGenerale, r => r.NumeroDepense, r => r.ExcelRow, SheetName, "N° Dépense", errors);

        if (last <= 1)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = "La feuille ne contient aucune ligne de données (seul l'en-tête est présent)." });
        }
        else if (data.FicheGenerale.Count == 0)
        {
            errors.Add(new ImportErrorDto { Sheet = SheetName, RowNumber = 0, Column = "-", Message = $"{last - 1} ligne(s) trouvée(s) mais toutes vides ou incomplètes (N° Dépense, Exercice et Fournisseur tous vides sur chacune) — vérifiez que le fichier a bien été rempli." });
        }
    }
}