using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_DOTATION_LIGNESBUDGETAIRE")]
public class BdgDotationLigneBudgetaire
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_DOTATION")]
    public decimal CodeDotation { get; set; }

    [Column("CODE_LIGNE")] public decimal? CodeLigne { get; set; }
    [Column("ANNEE")] public decimal? Annee { get; set; }
    [Column("CREDIT_DISPONIBLE_CP")] public decimal? CreditDisponibleCp { get; set; }
    [Column("CREDIT_DISPONIBLE_CE")] public double? CreditDisponibleCe { get; set; }
    [Column("CREDIT_PRE_ENGAGE_CP")] public double? CreditPreEngageCp { get; set; }
    [Column("CREDIT_PRE_ENGAGE_CE")] public double? CreditPreEngageCe { get; set; }
    [Column("CREDIT_ENGAGE_CP")] public double? CreditEngageCp { get; set; }
    [Column("CREDIT_ENGAGE_CE")] public double? CreditEngageCe { get; set; }
    [Column("compteur_Cp")] public decimal? CompteurCp { get; set; }
    [Column("compteur_Ce")] public decimal? CompteurCe { get; set; }
}

