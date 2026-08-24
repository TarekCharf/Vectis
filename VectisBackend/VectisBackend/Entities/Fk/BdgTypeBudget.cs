using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_TYPE_BUDGET")]
public class BdgTypeBudget
{
    [Key]
    [Column("CODE_TYPE_BDG")]
    public decimal CodeTypeBdg { get; set; }

    [Column("DESIGNATION_TYPE_BDG")] public string? DesignationTypeBdg { get; set; }
}