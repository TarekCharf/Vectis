using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_FACTURE_RELEVE")]
public class DepFactureReleve
{
    [Column("CODE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal CodeFacture { get; set; }

    [Column("CODE_RELEVE", TypeName = "numeric(10,0)")]
    public decimal CodeReleve { get; set; }
}