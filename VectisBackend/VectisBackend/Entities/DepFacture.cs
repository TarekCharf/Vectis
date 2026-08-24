using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_FACTURES")]
public class DepFacture
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal CodeFacture { get; set; }

    [Column("NUMERO_ORDRE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal NumeroOrdreFacture { get; set; }

    [Column("NUMERO_FACTURE"), StringLength(100)]
    public string NumeroFacture { get; set; } = null!;

    [Column("CODE_FOURNISSEUR", TypeName = "numeric(38,0)")]
    public decimal? CodeFournisseur { get; set; }

    [Column("CODE_MARCHE", TypeName = "numeric(10,0)")]
    public decimal CodeMarche { get; set; }

    [Column("REFERENCE_FACTURE"), StringLength(500)]
    public string? ReferenceFacture { get; set; }

    [Column("DATE_FACTURE")]
    public DateTime? DateFacture { get; set; }

    [Column("DateArrive", TypeName = "date")]
    public DateTime? DateArrive { get; set; }

    [Column("MOIS", TypeName = "numeric(18,0)")]
    public decimal? Mois { get; set; }

    [Column("CODE_TYPE_CONTRAT", TypeName = "numeric(38,0)")]
    public decimal? CodeTypeContrat { get; set; }

    [Column("TYPE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal? TypeFacture { get; set; }

    [Column("MONTANT_REGULARISATION_TTC", TypeName = "numeric(20,4)")]
    public decimal? MontantRegularisationTtc { get; set; }

    [Column("MONTANT_REGULARISATION_RS", TypeName = "numeric(20,4)")]
    public decimal? MontantRegularisationRs { get; set; }

    [Column("MONTANT_REGULARISATION_TVA", TypeName = "numeric(20,4)")]
    public decimal? MontantRegularisationTva { get; set; }

    [Column("MONTANT_REGULARISATION_HT", TypeName = "numeric(20,4)")]
    public decimal? MontantRegularisationHt { get; set; }

    [Column("MONTANT_FACTURE_FRS_TTC", TypeName = "numeric(18,2)")]
    public decimal? MontantFactureFrsTtc { get; set; }

    [Column("MONTANT_FACTURE_FRS_HT", TypeName = "numeric(18,2)")]
    public decimal? MontantFactureFrsHt { get; set; }

    [Column("CalculRGTOTAL", TypeName = "numeric(1,0)")]
    public decimal? CalculRgtotal { get; set; }

    [Column("CALCUL_PENALTES", TypeName = "numeric(38,0)")]
    public decimal? CalculPenaltes { get; set; }

    [Column("TAUX_RS_SUR_TVA_APPLIQUE")]
    public int? TauxRsSurTvaApplique { get; set; }

    [Column("Penalite_Spe", TypeName = "numeric(10,2)")]
    public decimal? PenaliteSpe { get; set; }

    [Column("Nbre_Jr_Mnq", TypeName = "numeric(10,2)")]
    public decimal? NbreJrMnq { get; set; }

    [Column("Indemnisation", TypeName = "numeric(10,2)")]
    public decimal? Indemnisation { get; set; }

    [Column("OBSERVATION_SERVICE_FAIT"), StringLength(400)]
    public string? ObservationServiceFait { get; set; }

    [Column("JUSTIFICATION"), StringLength(1000)]
    public string? Justification { get; set; }

    [Column("VALIDER", TypeName = "numeric(38,0)")]
    public decimal? Valider { get; set; }

    [Column("DATE_VALIDATION")]
    public DateTime? DateValidation { get; set; }

    [Column("ORDONNONCEE", TypeName = "numeric(38,0)")]
    public decimal Ordonnoncee { get; set; }
}