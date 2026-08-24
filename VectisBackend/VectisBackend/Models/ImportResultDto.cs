namespace VectisBackend.Models;

public class ImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int InsertedRows { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new();
}