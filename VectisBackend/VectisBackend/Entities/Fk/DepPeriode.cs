using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_PERIODE")]
public class DepPeriode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_PERIODE", TypeName = "numeric(10,0)")]
    public decimal CodePeriode { get; set; }

    [Column("LIBELLE_PERIODE"), StringLength(255)]
    public string LibellePeriode { get; set; } = default!;
}
