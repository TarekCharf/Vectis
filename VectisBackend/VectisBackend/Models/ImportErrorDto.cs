namespace VectisBackend.Models;

public class ImportErrorDto
{
    public string Sheet { get; set; } = "";
    public int RowNumber { get; set; }
    public string Column { get; set; } = "";
    public string Message { get; set; } = "";
}