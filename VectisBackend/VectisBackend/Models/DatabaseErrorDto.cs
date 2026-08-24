namespace VectisBackend.Models;

public class DatabaseErrorDto
{
    public string Sheet { get; set; } = "Database";
    public int RowNumber { get; set; } = 0;
    public string Column { get; set; } = "-";
    public string Message { get; set; } = "";
    public string ErrorType { get; set; } = ""; // "Constraint", "Connection", "Timeout", etc.
    public string SqlErrorCode { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string TechnicalMessage { get; set; } = ""; // Pour les logs
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Propriétés calculées pour la compatibilité
    public bool IsWarning { get; set; } = false;
    public bool IsFatal { get; set; } = false; // Si l'erreur empêche toute la transaction
}