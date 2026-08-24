using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_TYPE_PRESTATION")]
public class DepTypePrestation
{
    [Key]
    [Column("CODE_TYPE_PRESTATION")]
    public int CodeTypePrestation { get; set; }

    [Column("DESIGNATION")] public string? Designation { get; set; }
}