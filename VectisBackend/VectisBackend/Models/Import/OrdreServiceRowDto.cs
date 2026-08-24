namespace VectisBackend.Models.Import;

public class OrdreServiceRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public string NumeroOrdreService { get; set; } = "";
    public DateTime? DateOs { get; set; }
    public DateTime? DateNotificationOs { get; set; }
    public DateTime? DateEffetOs { get; set; }
    
}