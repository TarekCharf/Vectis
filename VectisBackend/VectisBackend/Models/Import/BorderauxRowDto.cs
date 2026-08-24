namespace VectisBackend.Models.Import;

public class BordereauRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public string DesignationProduit { get; set; } = "";
    public decimal? Quantite { get; set; }
    public decimal? PuMinimum { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? Tva { get; set; }
    public string? Remise { get; set; }       // Oui/Non
    public string? PrixRevisable { get; set; } // Oui/Non
    public string? Rs { get; set; }           // Oui/Non
    public string? RsSurTva { get; set; }     // Oui/Non
}