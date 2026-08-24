using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_MAGASIN")]
public class GenMagasin
{
    [Key]
    [Column("CODE_MAGASIN")]
    public decimal CodeMagasin { get; set; }

    [Column("LIBELLE")]
    public string? Libelle { get; set; }

    [Column("MAGASIN_ID")]
    public string MagasinId { get; set; } = null!;

    [Column("CODE_VILLE")]
    public decimal? CodeVille { get; set; }

    [Column("magasin_reforme")]
    public int? MagasinReforme { get; set; }
}
