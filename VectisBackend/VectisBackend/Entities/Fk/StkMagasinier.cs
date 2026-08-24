using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("STK_MAGASINIER")]
public class StkMagasinier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_MAGASINIER")]
    public decimal CodeMagasinier { get; set; }

    [Column("NOM")]
    public string? Nom { get; set; }

    [Column("PRENOM")]
    public string? Prenom { get; set; }

    [Column("ID")]
    public decimal? Id { get; set; }

    [Column("CODE_TYPE_MAGASINIER")]
    public decimal? CodeTypeMagasinier { get; set; }
}
