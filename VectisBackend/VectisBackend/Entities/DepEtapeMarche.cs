using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_ETAPE_MARCHE")]
public class DepEtapeMarche
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_ETAPE_MARCHE", TypeName = "numeric(38,0)")]

    public decimal CodeEtapeMarche { get; set; }

    [Column("CODE_ETAPE", TypeName = "numeric(38,0)")]
    public decimal? CodeEtape { get; set; }

    [Column("CODE_MARCHE", TypeName = "numeric(38,0)")]
    public decimal? CodeMarche { get; set; }

    [Column("SELECTIONNER"), StringLength(2)]
    public string? Selectionner { get; set; }

    [Column("DATE_ETAPE", TypeName = "datetime")]
    public DateTime? DateEtape { get; set; }

    [Column("REFERENCE"), StringLength(100)]
    public string? Reference { get; set; }

    [Column("NETAPE", TypeName = "numeric(38,0)")]
    public decimal? Netape { get; set; }

    [Column("DATE_ETAPE_PREVISIONNELLE", TypeName = "date")]
    public DateTime? DateEtapePrevisionnelle { get; set; }
}
