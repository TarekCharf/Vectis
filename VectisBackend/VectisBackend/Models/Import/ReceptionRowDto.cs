namespace VectisBackend.Models.Import;

public class ReceptionRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public DateTime? DateReception { get; set; }
    public string NatureReception { get; set; } = "";
    public string TypeReception { get; set; } = "";
    public string Lieux { get; set; } = "";
    public string NumFicheReception { get; set; } = "";
    public string? HeureReception { get; set; } = "";
    public string NumeroLivraison { get; set; } = "";
    public string Validation { get; set; } = ""; // Oui/Non
    public DateTime? DateValidation { get; set; }
}
