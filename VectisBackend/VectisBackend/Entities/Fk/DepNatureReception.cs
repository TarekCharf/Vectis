using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_NATURE_RECEPTION")]
public class DepNatureReception
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_NATURERECEPTION")]
    public decimal CodeNatureReception { get; set; }

    [Column("LIBELLE_NATURERECEPTION")]
    public string LibelleNatureReception { get; set; } = null!;
}
