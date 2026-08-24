using Microsoft.AspNetCore.Http;

namespace VectisBackend.Models;

public class ImportDepenseFormRequest
{
    public IFormFile File { get; set; } = default!;

    public bool IncludeEngagement { get; set; } = false;
    public bool EngagementAvecValidation { get; set; } = false;

    public bool IncludeLivraison { get; set; } = false;
    public bool LivraisonAvecValidation { get; set; } = false;
    public bool IncludeReception { get; set; } = false;
    public bool ReceptionAvecValidation { get; set; } = false;

    public bool IncludeFacture { get; set; } = false;
    public bool FactureAvecValidation { get; set; } = false;
}