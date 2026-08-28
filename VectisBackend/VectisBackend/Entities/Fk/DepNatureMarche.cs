using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_NATURE_MARCHE")]
public class DepNatureMarche
{
    [Key]
    [Column("CODE_NATURE_MARCHE")]
    public decimal CodeNatureMarche { get; set; }

    [Column("LIBELLE")]
    public string? Libelle { get; set; }
}
