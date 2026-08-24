namespace VectisBackend.Models;

public class ExportDepenseRequestDto
{
    public string TypeDepense { get; set; } = "Marché/BC/CDC";

    public bool IncludeEngagement { get; set; } = false;
    public bool EngagementAvecValidation { get; set; } = false;

    public bool IncludeLivraison { get; set; } = false;
    public bool LivraisonAvecValidation { get; set; } = false;
    public bool IncludeReception { get; set; } = false;
    public bool ReceptionAvecValidation { get; set; } = false;

    public bool IncludeFacture { get; set; } = false;
    public bool FactureAvecValidation { get; set; } = false;
}
