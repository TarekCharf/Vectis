
using Microsoft.AspNetCore.Diagnostics;

namespace VectisBackend.Middleware;

/// <summary>
/// Filet de sécurité : toute exception qui, jusqu'ici, échappait aux try/catch locaux de
/// ImportDepenseService (par ex. un appel à _lookup.ResolveXxxAsync non entouré de try/catch)
/// finissait en page HTML (Developer Exception Page) ou en ProblemDetails minimal — dans les
/// deux cas illisible pour le front Angular, qui s'attend à du JSON avec "message"/"errors".
/// Ce handler intercepte tout ce qui n'a pas été géré plus bas et renvoie systématiquement
/// la même forme que ImportResultDto, pour que ImportDepenseService.normalizeError() (Cas 2
/// côté Angular) le reconnaisse toujours, quel que soit l'endroit où ça a explosé.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static void UseGlobalJsonExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json; charset=utf-8";

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error;

                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");
                logger.LogError(ex, "Exception non gérée sur {Path}", feature?.Path);

                // Même forme que ImportResultDto, pour que le front la traite comme un cas
                // "erreurs" standard plutôt que comme un système error générique.
                var payload = new
                {
                    success = false,
                    totalRows = 0,
                    insertedRows = 0,
                    errors = new[]
                    {
                        new
                        {
                            sheet = "Système",
                            rowNumber = 0,
                            column = "-",
                            message = app.Environment.IsDevelopment()
                                ? ex?.Message ?? "Erreur inattendue."
                                : "Une erreur inattendue est survenue côté serveur. Contactez le support si le problème persiste."
                        }
                    }
                };

                await context.Response.WriteAsJsonAsync(payload);
            });
        });
    }
}
