namespace VectisBackend.Services;

/// <summary>État d'une étape du workflow (Engagement, Livraison, Réception, Facture).</summary>
public sealed record StepSelection(string Key, bool Included, bool AvecValidation);

/// <summary>Options d'export envoyées par le front (équivalent de getStepsState() en JS).</summary>
public sealed record ExportOptions(string TypeDepense, IReadOnlyList<StepSelection> Steps);

public interface IExportTemplateService
{
    Task<byte[]> BuildImportTemplateAsync(ExportOptions options, CancellationToken ct = default);
}