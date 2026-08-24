namespace VectisBackend.Models.Import;

public class LivraisonRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public string NumeroLivraison { get; set; } = "";
    public string ResponsableLivraison { get; set; } = "";
    public string? Magasin { get; set; } = "";
    public string? Magasinier { get; set; } = "";
    public DateTime? DateLivraison { get; set; }
    public string? HeureLivraison { get; set; } = "";
    public DateTime? DateLivraisonPrevisionnelle { get; set; }
    public DateTime? DateDepot { get; set; }
    public string ProduitNPrix { get; set; } = "";
    public decimal? ProduitQuantiteLivree { get; set; }
    public decimal? ProduitQuantiteAcceptee { get; set; }
    public string Validation { get; set; } = ""; // Oui/Non
    public DateTime? DateValidation { get; set; }
}
