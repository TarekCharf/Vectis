using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("Dep_charge_Suivi")]
public class DepChargeSuivi
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Code_charge_Suivi", TypeName = "numeric(18,0)")]
    public decimal CodeChargeSuivi { get; set; }

    [Column("Mission"), StringLength(200)]
    public string? Mission { get; set; }

    [Column("Code_agent", TypeName = "numeric(18,0)")]
    public decimal? CodeAgent { get; set; }

    [Column("code_marche", TypeName = "numeric(10,0)")]
    public decimal? CodeMarche { get; set; }
}