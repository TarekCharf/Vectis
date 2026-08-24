using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_EXERCICE")]
public class BdgExercice
{
    [Key]
    [Column("CODE_EXERCIE")]
    public decimal CodeExercie { get; set; }

    [Column("ANNEE")] public decimal? Annee { get; set; }
    [Column("OUVERT")] public decimal? Ouvert { get; set; }
    [Column("CLOTURE")] public decimal? Cloture { get; set; }
}