using Microsoft.AspNetCore.Mvc;
using VectisBackend.Models;
using VectisBackend.Services;

namespace VectisBackend.Controllers;

[ApiController]
[Route("api/export-depense")]
public class ExportTemplateController : ControllerBase
{
    private readonly IExportTemplateService _exportService;

    public ExportTemplateController(IExportTemplateService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Génère le fichier Excel d'import dépense, avec les étapes (Engagement/Livraison/
    /// Réception/Facture) et leur mode de validation choisis dans le corps de la requête —
    /// même logique que le stepper de import-depense.html. Endpoint prévu pour être appelé
    /// depuis le futur front Angular ; testable dès maintenant via Swagger.
    /// </summary>
    [HttpPost("excel")]
    public async Task<IActionResult> GetImportDepenseTemplate([FromBody] ExportDepenseRequestDto request, CancellationToken ct)
    {
        // Auto-correction des combinaisons invalides, comme applyConstraints() le fait en
        // direct dans import-depense.html (ex: Livraison "Avec validation" sans Engagement
        // validé est automatiquement ramenée à "Sans validation").
        var constrained = StepSelectionConstraints.Apply(new StepSelectionConstraints.Input(
            request.IncludeEngagement, request.EngagementAvecValidation,
            request.IncludeLivraison, request.LivraisonAvecValidation,
            request.IncludeReception, request.ReceptionAvecValidation,
            request.IncludeFacture, request.FactureAvecValidation));

        var options = BuildExportOptions(request.TypeDepense, constrained);

        var bytes = await _exportService.BuildImportTemplateAsync(options, ct);
        var fileName = $"import-depense_{request.TypeDepense}_{DateTime.Now:dd-MM-yyyy}.xlsx";

        if (constrained.WasAdjusted)
            Response.Headers.Append("X-Export-Adjustments", string.Join(" | ", constrained.Adjustments));

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static ExportOptions BuildExportOptions(string typeDepense, StepSelectionConstraints.Result c) =>
        new(
            TypeDepense: typeDepense,
            Steps: new List<StepSelection>
            {
                new("engagement", c.EngagementIncluded, c.EngagementAvecValidation),
                new("livraison",  c.LivraisonIncluded,  c.LivraisonAvecValidation),
                new("reception",  c.ReceptionIncluded,  c.ReceptionAvecValidation),
                new("facture",    c.FactureIncluded,    c.FactureAvecValidation),
            });
}
