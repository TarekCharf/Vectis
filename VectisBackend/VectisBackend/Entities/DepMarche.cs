using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_MARCHE")]
public class DepMarche
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_MARCHE", TypeName = "numeric(10,0)")]
    public decimal CodeMarche { get; set; }

    [Column("CODE_TYPE_CONTRAT", TypeName = "numeric(10,0)")]
    public decimal? CodeTypeContrat { get; set; }

    [Column("EXERCICE"), StringLength(50)]
    public string? Exercice { get; set; }

    [Column("NUMERO_MARCHE"), StringLength(255)]
    public string NumeroMarche { get; set; } = default!;

    [Column("CodeTypePrestation")]
    public int? CodeTypePrestation { get; set; }

    [Column("NATURE_PRESTATION", TypeName = "numeric(38,0)")]
    public decimal? NaturePrestation { get; set; }

    [Column("CODE_TYPE_BUDGET", TypeName = "numeric(38,0)")]
    public decimal? CodeTypeBudget { get; set; }

    [Column("LIBELLE_MARCHE"), StringLength(2000)]
    public string? LibelleMarche { get; set; }

    [Column("OBSERVATION_MARCHE"), StringLength(2500)]
    public string? ObservationMarche { get; set; }

    [Column("CODE_FOURNISSEUR", TypeName = "numeric(10,0)")]
    public decimal CodeFournisseur { get; set; }

    [Column("REPRESENTANT"), StringLength(4000)]
    public string? Representant { get; set; }

    [Column("CODECENTREGES", TypeName = "numeric(12,0)")]
    public decimal? CodeCentreGes { get; set; }

    [Column("CODE_MO", TypeName = "numeric(10,0)")]
    public decimal? CodeMO { get; set; }

    [Column("DATE_NOTIFICATION")]
    public DateTime? DateNotification { get; set; }

    [Column("CODE_DEVISE", TypeName = "numeric(18,0)")]
    public decimal? CodeDevise { get; set; }

    [Column("DELAI_EXECUTION", TypeName = "numeric(10,0)")]
    public decimal? DelaiExecution { get; set; }

    [Column("DELAI_GARANTIE", TypeName = "numeric(10,0)")]
    public decimal? DelaiGarantie { get; set; }

    [Column("UNITE_DELAI_EXECUTION", TypeName = "numeric(10,0)")]
    public decimal? UniteDelaiExecution { get; set; }

    [Column("UNITE_DELAI_GARANTIE", TypeName = "numeric(10,0)")]
    public decimal? UniteDelaiGarantie { get; set; }

    [Column("CODE_MODE_PAIEMENT", TypeName = "numeric(10,0)")]
    public decimal? CodeModePaiement { get; set; }

    [Column("CODE_BANQUE_PAIEMENT", TypeName = "numeric(38,0)")]
    public decimal? CodeBanquePaiement { get; set; }

    [Column("NUM_COMPTE_PAIEMENT"), StringLength(500)]
    public string? NumComptePaiement { get; set; }

    [Column("PERSONNE_HABILITEE", TypeName = "numeric(18,0)")]
    public decimal? PersonneHabilitee { get; set; }

    [Column("VALIDE", TypeName = "numeric(10,0)")]
    public decimal? Valide { get; set; }

    [Column("Date_Achevement")]
    public DateTime? DateAchevement { get; set; }

    // Champs ajoutés depuis le second bloc SQL
    [Column("TAUX_TVA", TypeName = "real")]
    public float? TauxTva { get; set; }

    [Column("MONTANT_MARCHE_HT", TypeName = "numeric(20,2)")]
    public decimal? MontantMarcheHt { get; set; }

    [Column("MONTANT_MARCHE_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontantMarcheTtc { get; set; }

    [Column("MT_MARCHE_RABAIS", TypeName = "numeric(20,2)")]
    public decimal? MtMarcheRabais { get; set; }

    [Column("TAUX_RETENUE_SOURCE", TypeName = "real")]
    public float? TauxRetenueSource { get; set; }

    [Column("TAUX_RABAIS", TypeName = "real")]
    public float? TauxRabais { get; set; }

    [Column("RABAIS", TypeName = "numeric(1,0)")]
    public decimal? Rabais { get; set; }
}