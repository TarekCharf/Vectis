namespace VectisBackend.Models.Import;

public class EngagementRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public string CategorieBudget { get; set; } = "";
    public DateTime? DateEngagement { get; set; }
    public string CodeLigneBudgetaire { get; set; } = "";
    public decimal? MontantEngagerCP { get; set; }
    public decimal? MontantEngagerCE { get; set; }
    public string Validation { get; set; } = ""; // Oui/Non
    public DateTime? DateValidation { get; set; }
}

