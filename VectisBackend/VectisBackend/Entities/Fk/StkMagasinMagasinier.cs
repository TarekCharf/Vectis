using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("STK_MAGASIN_MAGASINIER")]
public class StkMagasinMagasinier
{
    [Column("CODE_MAGASIN_MAGASINIER")]
    public int? CodeMagasinMagasinier { get; set; }

    [Column("CODE_MAGASIN")]
    public int? CodeMagasin { get; set; }

    [Column("CODE_MAGASINIER")]
    public int? CodeMagasinier { get; set; }

    [Column("CODE_TYPE_MAGASINIER")]
    public int? CodeTypeMagasinier { get; set; }
}
