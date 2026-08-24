using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_MARCHE_CENTRE")]
public class DepMarcheCentre
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_LIGNE", TypeName = "numeric(18,0)")]
    public decimal CodeLigne { get; set; }

    [Column("CODE_MARCHE", TypeName = "numeric(10,0)")]
    public decimal? CodeMarche { get; set; }

    [Column("CODE_CENTRE", TypeName = "numeric(12,0)")]
    public decimal CodeCentre { get; set; }
}