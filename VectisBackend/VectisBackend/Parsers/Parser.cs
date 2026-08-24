using ClosedXML.Excel;
using System.Globalization;
using VectisBackend.Models;

namespace VectisBackend.Parsers;

public abstract class Parser : IExcelSheetParser
{
    public abstract string SheetName { get; }

    public abstract void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors);
    protected static void AddDuplicateErrors<T>(
        IEnumerable<T> rows,
        Func<T, string?> keySelector,
        Func<T, int> excelRowSelector,
        string sheet,
        string column,
        List<ImportErrorDto> errors)
    {
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var key = keySelector(row)?.Trim();
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var excelRow = excelRowSelector(row);

            if (seen.TryGetValue(key, out var firstRow))
            {
                errors.Add(new ImportErrorDto
                {
                    Sheet = sheet,
                    RowNumber = excelRow,
                    Column = column,
                    Message = $"Doublon : '{key}' apparaît déjà à la ligne {firstRow}."
                });
            }
            else
            {
                seen[key] = excelRow;
            }
        }
    }
    protected static DateTime? TryDate(
        IXLCell c,
        string sheet,
        int rowNumber,
        string column,
        List<ImportErrorDto> errors)
    {
        // Cellule vide => pas d'erreur de format
        // L'obligation éventuelle est vérifiée séparément.
        if (c.IsEmpty())
            return null;

        // --------------------------------------------------------
        // 1. Vraie date Excel
        // --------------------------------------------------------

        if (c.TryGetValue<DateTime>(out var date))
        {
            date = date.Date;

            if (date.Year < 1753)
            {
                errors.Add(new ImportErrorDto
                {
                    Sheet = sheet,
                    RowNumber = rowNumber,
                    Column = column,
                    Message = "Date invalide. La date doit être supérieure ou égale au 01/01/1753."
                });

                return null;
            }

            return date;
        }

        // --------------------------------------------------------
        // 2. Date sous forme de texte
        // Formats acceptés :
        // dd/MM/yy
        // dd-MM-yy
        // --------------------------------------------------------

        if (c.TryGetValue<string>(out var s))
        {
            s = s.Trim();

            if (string.IsNullOrWhiteSpace(s))
                return null;

            string[] formats =
            {
                "dd/MM/yy",
                "dd-MM-yy"
            };

            if (DateTime.TryParseExact(
                s,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
            {
                parsedDate = parsedDate.Date;

                if (parsedDate.Year < 1753)
                {
                    errors.Add(new ImportErrorDto
                    {
                        Sheet = sheet,
                        RowNumber = rowNumber,
                        Column = column,
                        Message = "Date invalide. La date doit être supérieure ou égale au 01/01/1753."
                    });

                    return null;
                }

                return parsedDate;
            }

            // Texte mais mauvais format
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Format de date invalide. Format attendu : jj/mm/aa ou jj-mm-aa."
            });

            return null;
        }

        // --------------------------------------------------------
        // 3. Autre type de cellule
        // --------------------------------------------------------

        errors.Add(new ImportErrorDto
        {
            Sheet = sheet,
            RowNumber = rowNumber,
            Column = column,
            Message = "Format invalide. La valeur doit être une date Excel ou une date au format jj/mm/aa ou jj-mm-aa."
        });

        return null;
    }
    protected static string? TryTime(
        IXLCell c,
        string sheet,
        int rowNumber,
        string column,
        List<ImportErrorDto> errors)
    {
        if (c.IsEmpty())
            return null;

        // Heure Excel
        if (c.TryGetValue<TimeSpan>(out var time))
        {
            if (time.TotalMinutes >= 0 && time.TotalMinutes <= 1439)
                return time.ToString(@"hh\:mm");

            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Heure invalide. L'heure doit être comprise entre 00:00 et 23:59."
            });

            return null;
        }

        // Heure sous forme de texte
        if (c.TryGetValue<string>(out var s))
        {
            s = s.Trim();

            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (TimeSpan.TryParseExact(
                s,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var parsedTime))
            {
                if (parsedTime.TotalMinutes >= 0 && parsedTime.TotalMinutes <= 1439)
                    return parsedTime.ToString(@"hh\:mm");
            }

            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Format d'heure invalide. Format attendu : HH:mm, entre 00:00 et 23:59."
            });

            return null;
        }

        errors.Add(new ImportErrorDto
        {
            Sheet = sheet,
            RowNumber = rowNumber,
            Column = column,
            Message = "Format invalide. La valeur doit être une heure au format HH:mm."
        });

        return null;
    }
    protected static decimal? TryDecimal(
        IXLCell c,
        string sheet,
        int rowNumber,
        string column,
        List<ImportErrorDto> errors)
    {
        // Cellule vide = null, pas d'erreur de format
        if (c.IsEmpty())
            return null;

        // --------------------------------------------------------
        // La valeur doit être numérique
        // --------------------------------------------------------

        if (c.TryGetValue<decimal>(out var value))
            return value;

        errors.Add(new ImportErrorDto
        {
            Sheet = sheet,
            RowNumber = rowNumber,
            Column = column,
            Message = "Format invalide. La valeur doit être numérique."
        });

        return null;
    }
    protected static float? TryFloat(
        IXLCell c,
        string sheet,
        int rowNumber,
        string column,
        List<ImportErrorDto> errors)
    {
        // Cellule vide = null, pas d'erreur de format
        if (c.IsEmpty())
            return null;

        // La valeur doit être numérique
        if (c.TryGetValue<float>(out var value))
            return value;

        errors.Add(new ImportErrorDto
        {
            Sheet = sheet,
            RowNumber = rowNumber,
            Column = column,
            Message = "Format invalide. La valeur doit être numérique."
        });

        return null;
    }
    protected static string? TryDuration(
        IXLCell c,
        string sheet,
        int rowNumber,
        string column,
        List<ImportErrorDto> errors)
    {
        if (c.IsEmpty())
            return null;

        if (!c.TryGetValue<string>(out var s))
        {
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Format invalide. Format attendu : nombre + période (ex: 1 Jour, 2 Mois, 3 Années)."
            });

            return null;
        }

        s = s.Trim();

        if (string.IsNullOrWhiteSpace(s))
            return null;

        // Séparation du nombre et de la période
        var parts = s.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
        {
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Format invalide. Format attendu : nombre + période (ex: 1 Jour, 2 Mois, 3 Années)."
            });

            return null;
        }

        // Nombre
        if (!decimal.TryParse(
            parts[0].Replace(',', '.'),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var nombre))
        {
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = $"Nombre invalide : '{parts[0]}'."
            });

            return null;
        }

        if (nombre <= 0)
        {
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = "Le nombre doit être supérieur à 0."
            });

            return null;
        }

        // Période
        var periode = parts[1].Trim();

        var periodesValides = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["jour"] = "Jour",
            ["jours"] = "Jour",

            ["mois"] = "Mois",

            ["semestre"] = "Semestre",
            ["semestres"] = "Semestre",

            ["trimestre"] = "Trimestre",
            ["trimestres"] = "Trimestre",

            ["année"] = "Année",
            ["années"] = "Année",
            ["annee"] = "Année",
            ["annees"] = "Année"
        };

        if (!periodesValides.TryGetValue(periode, out var periodeNormalisee))
        {
            errors.Add(new ImportErrorDto
            {
                Sheet = sheet,
                RowNumber = rowNumber,
                Column = column,
                Message = $"Période invalide : '{parts[1]}'. Périodes acceptées : Jour, Mois, Semestre, Trimestre, Année."
            });

            return null;
        }

        return $"{nombre} {periodeNormalisee}";
    }
}
