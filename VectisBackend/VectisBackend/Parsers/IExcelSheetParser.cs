using ClosedXML.Excel;
using VectisBackend.Models;

namespace VectisBackend.Parsers;

public interface IExcelSheetParser
{
    string SheetName { get; }
    void Parse(XLWorkbook workbook, ImportDepenseRequestDto ctx, WorkbookImportDto data, List<ImportErrorDto> errors);
}