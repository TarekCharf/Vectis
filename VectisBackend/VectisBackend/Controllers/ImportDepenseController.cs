using Microsoft.AspNetCore.Mvc;
using VectisBackend.Models;
using VectisBackend.Services;

namespace VectisBackend.Controllers;

[ApiController]
[Route("api/import-depense")]
public class ImportDepenseController : ControllerBase
{
    private readonly IImportDepenseService _importService;

    public ImportDepenseController(IImportDepenseService importService)
    {
        _importService = importService;
    }

    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportExcel([FromForm] ImportDepenseFormRequest form, CancellationToken ct)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest("Fichier obligatoire.");

        // Mêmes règles de dépendance que l'export (voir StepSelectionConstraints) — mais ici on
        // REJETTE la combinaison plutôt que de l'auto-corriger : cet endpoint écrit en base,
        // mieux vaut que l'appelant corrige explicitement sa requête qu'un comportement modifié
        // silencieusement derrière son dos.
        var constrained = StepSelectionConstraints.Apply(new StepSelectionConstraints.Input(
            form.IncludeEngagement, form.EngagementAvecValidation,
            form.IncludeLivraison, form.LivraisonAvecValidation,
            form.IncludeReception, form.ReceptionAvecValidation,
            form.IncludeFacture, form.FactureAvecValidation));

        if (constrained.WasAdjusted)
        {
            return BadRequest(new
            {
                message = "Combinaison Engagement/Livraison/Réception/Facture invalide.",
                details = constrained.Adjustments
            });
        }

        var dto = new ImportDepenseRequestDto
        {
            IncludeEngagement = form.IncludeEngagement,
            EngagementAvecValidation = form.EngagementAvecValidation,
            IncludeLivraison = form.IncludeLivraison,
            LivraisonAvecValidation = form.LivraisonAvecValidation,
            IncludeReception = form.IncludeReception,
            ReceptionAvecValidation = form.ReceptionAvecValidation,
            IncludeFacture = form.IncludeFacture,
            FactureAvecValidation = form.FactureAvecValidation
        };

        var result = await _importService.ImportAsync(form.File, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
