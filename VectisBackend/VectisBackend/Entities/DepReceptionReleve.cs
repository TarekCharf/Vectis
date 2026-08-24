using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_RECEPTION_RELEVE")]
public class DepReceptionReleve
{
    [Column("CODE_RECEPTION")]
    public int CodeReception { get; set; }

    [Column("CODE_RELEVE", TypeName = "numeric(10,0)")]
    public decimal CodeReleve { get; set; }
}