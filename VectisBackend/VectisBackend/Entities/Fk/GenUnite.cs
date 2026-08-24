using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_UNITE")]
public class GenUnite
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_UNITE")]
    public decimal CodeUnite { get; set; }

    [Column("DESIGNATION")] public string? Designation { get; set; }
}