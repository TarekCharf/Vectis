using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_DEVISE")]
public class GenDevise
{
    [Key]
    [Column("CODE_DEVISE")]
    public decimal CodeDevise { get; set; }

    [Column("DEVISE")] public string? Devise { get; set; }
}