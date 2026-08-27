using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_ETAPE")]
public class GenEtape
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_ETAPE", TypeName = "numeric(38,0)")]

    public decimal CodeEtape { get; set; }

    [Column("ORDRE_AFFICHAGE", TypeName = "numeric(38,0)")]
    public decimal? OrdreAffichage { get; set; }

    [Column("TYPE_MARCHE", TypeName = "numeric(38,0)")]
    public decimal? TypeMarche { get; set; }

    [Column("NETAPE", TypeName = "numeric(38,0)")]
    public decimal? Netape { get; set; }

    [Column("TYPE_BUDGET", TypeName = "numeric(38,0)")]
    public decimal? TypeBudget { get; set; }

    [Column("CODE_NATURE_MARCHE", TypeName = "numeric(38,0)")]
    public decimal? CodeNatureMarche { get; set; }
}
