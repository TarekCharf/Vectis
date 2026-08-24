using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("CPT_ENGAGEMENT_LIGNE")]
public class CptEngagementLigne
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_ENGAGEMENT_LIGNE", TypeName = "numeric(38,0)")]
    public decimal CodeEngagementLigne { get; set; }

    [Column("CODE_ENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal? CodeEngagement { get; set; }

    [Column("CODE_DEPENSE", TypeName = "numeric(10,0)")]
    public decimal? CodeDepense { get; set; }

    [Column("CODE_LIGNE", TypeName = "numeric(38,0)")]
    public decimal? CodeLigne { get; set; }

    [Column("MONTANT_ENGAGE_CP", TypeName = "numeric(20,4)")]
    public decimal? MontantEngageCp { get; set; }

    [Column("MONTANT_ENGAGE_CE", TypeName = "numeric(20,4)")]
    public decimal? MontantEngageCe { get; set; }

    [Column("DISPONIBLE_CP", TypeName = "numeric(20,4)")]
    public decimal? DisponibleCp { get; set; }

    [Column("DISPONIBLE_CE", TypeName = "numeric(20,4)")]
    public decimal? DisponibleCe { get; set; }

    [Column("MONTANT_MARCHE", TypeName = "numeric(20,4)")]
    public decimal? MontantMarche { get; set; }

    [Column("CREDIT_ENGAGE_CP", TypeName = "numeric(20,4)")]
    public decimal? CreditEngageCp { get; set; }

    [Column("CREDIT_ENGAGE_CE", TypeName = "numeric(20,4)")]
    public decimal? CreditEngageCe { get; set; }

    [Column("CREDIT_DEFINITIF_CP", TypeName = "numeric(20,4)")]
    public decimal? CreditDefinitifCp { get; set; }

    [Column("CREDIT_DEFINITIF_CE", TypeName = "numeric(20,4)")]
    public decimal? CreditDefinitifCe { get; set; }

    [Column("COMPTEUR_CP", TypeName = "numeric(38,0)")]
    public decimal? CompteurCp { get; set; }

    [Column("COMPTEUR_CE", TypeName = "numeric(38,0)")]
    public decimal? CompteurCe { get; set; }
}