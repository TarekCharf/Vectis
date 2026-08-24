using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_TYPE_RECEPTION")]
public class DepTypeReception
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_TYPERECEPTION")]
    public decimal CodeTypeReception { get; set; }

    [Column("LIBELLE_TYPERECEPTION")]
    public string LibelleTypeReception { get; set; } = null!;
}
