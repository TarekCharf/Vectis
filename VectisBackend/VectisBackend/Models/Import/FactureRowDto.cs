namespace VectisBackend.Models.Import;

public class FactureRowDto
{
    public int ExcelRow { get; set; }

    // Champs obligatoires
    public string NumeroDepense { get; set; } = "";             // N° Dépense (obligatoire:Oui)
    public string NumeroFactureFournisseur { get; set; } = "";  // N° facture fournisseur (obligatoire:Oui)
    public DateTime? DateFacture { get; set; }             // Date facture (obligatoire:Oui)
    public DateTime? DateDepotFacture { get; set; }        // Date dépôt facture (obligatoire:Oui)
    public string NumeroLivraison { get; set; } = "";           // N° Livraison (obligatoire:Oui)

    // Champs optionnels
    public string? Periode { get; set; }                        // Période
    public decimal? MontantRevision { get; set; }              // Montant Révision
    public string? CalculerRgTotal { get; set; }                // Calculer RG TOTAL (Oui/Non)
    public string? CalculerPenalitesRetard { get; set; }       // Calculer les pénalités de retard (Oui/Non)
    public string? PourcentageReception { get; set; }          // % Reception
    public string? PourcentageMarche { get; set; }             // % Marché
    public decimal? TauxRsSurTvaApplique { get; set; }          // Taux RS sur TVA Appliqué
    public string? PenaliteSpeciale { get; set; }              // Pénalité Spéciale
    public decimal? PenaliteSpecialeNbreJoursManquement { get; set; } // Pénalité Spéciale-Nombre de jour de manquement
    public decimal? PenaliteSpecialeIndemnisation { get; set; } // Pénalité Spéciale-Indemnisation
    public string? Observation { get; set; }                    // Observation
    public string? Justification { get; set; }                  // Justification
    public string Validation { get; set; } = ""; // Oui/Non
    public DateTime? DateValidation { get; set; }
}