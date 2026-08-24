using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_NATURE_PRESTATION")]
public class DepNaturePrestation
{
    [Key]
    [Column("CODE_NATURE")]
    public decimal CodeNature { get; set; }

    [Column("LIBELLE_NATURE")] public string? LibelleNature { get; set; }
    [Column("Code_Type_Prestation")] public int? CodeTypePrestation { get; set; }
    [Column("code_type_depense")] public decimal? CodeTypeDepense { get; set; }
    [Column("CODE_NATURE_PRESTATION")] public decimal? CodeNaturePrestation {  get; set; }
}