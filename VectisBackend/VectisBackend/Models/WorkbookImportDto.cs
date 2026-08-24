namespace VectisBackend.Models;

using Import;

public class WorkbookImportDto
{
    public List<FicheGeneraleRowDto> FicheGenerale { get; set; } = new();
    public List<BordereauRowDto> Bordereaux { get; set; } = new();
    public List<OrdreServiceRowDto> OrdresService { get; set; } = new();
    public List<EngagementRowDto> Engagements { get; set; } = new();
    public List<LivraisonRowDto> Livraisons { get; set; } = new();
    public List<ReceptionRowDto> Receptions { get; set; } = new();
    public List<FactureRowDto> Factures { get; set; } = new();
}