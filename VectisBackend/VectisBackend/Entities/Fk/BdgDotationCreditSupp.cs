using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_DOTATION_CREDITSUPP")]
public class BdgDotationCreditSupp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_DOTATION_CS")]
    public decimal CodeDotationCs { get; set; }

    [Column("CODE_TYPE_CREDIT")] public decimal? CodeTypeCredit { get; set; }
    [Column("CODE_LIGNE")] public decimal? CodeLigne { get; set; }
    [Column("ANNEE")] public string? Annee { get; set; }
    [Column("CREDIT_DISPONIBLE_CP")] public decimal? CreditDisponibleCp { get; set; }
    [Column("CREDIT_DISPONIBLE_CE")] public decimal? CreditDisponibleCe { get; set; }
    [Column("CREDIT_PRE_ENGAGE_CP")] public decimal? CreditPreEngageCp { get; set; }
    [Column("CREDIT_PRE_ENGAGE_CE")] public decimal? CreditPreEngageCe { get; set; }
    [Column("CREDIT_ENGAGE_CP")] public decimal? CreditEngageCp { get; set; }
    [Column("CREDIT_ENGAGE_CE")] public decimal? CreditEngageCe { get; set; }
}

