using Microsoft.AspNetCore.Http;
using VectisBackend.Models;

namespace VectisBackend.Services;

public interface IImportDepenseService
{
    Task<ImportResultDto> ImportAsync(IFormFile file, ImportDepenseRequestDto options, CancellationToken ct);
}