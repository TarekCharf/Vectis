namespace VectisBackend.Models.Import;

public class FicheGeneraleRowDto
{
    public int ExcelRow { get; set; }
    public string NumeroDepense { get; set; } = "";
    public string TypeContract { get; set; } = "";
    public string Exercice { get; set; } = "";
    public string? TypePrestation { get; set; }
    public string NaturePrestation { get; set; } = "";
    public string? TypeBudget { get; set; }
    public string Objet { get; set; } = "";
    public string? Observation { get; set; }
    public string Fournisseur { get; set; } = "";
    public string? Representant { get; set; }
    public string CentreGestionnaire { get; set; } = "";
    public string? MaitreOuvrage { get; set; }
    public DateTime? DateDepense { get; set; }
    public string CentreBeneficiaire { get; set; } = "";
    public string Devise { get; set; } = "";
    public string? ChargeSuiviAgent { get; set; }
    public string? ChargeSuiviMission { get; set; }
    public string? DelaiExecution { get; set; }
    public string? DelaiGarantie { get; set; }
    public string ModePaiement { get; set; } = "";
    public string? Banque { get; set; }
    public string? NumeroCompte { get; set; }
    public string? PersonneHabilitee { get; set; }
    public float? TauxRemise { get; set; }
    public float? TauxRetenueSource { get; set; }
    public string Validation { get; set; } = ""; // Oui/Non
    public DateTime? DateValidation { get; set; }
}