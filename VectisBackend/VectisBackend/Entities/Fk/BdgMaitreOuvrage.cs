using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_MAITRE_OUVRAGE")]
public class BdgMaitreOuvrage
{
    [Key]
    [Column("CODE_MAITRE_OUVRAGE")]
    public decimal CodeMaitreOuvrage { get; set; }

    [Column("NOM")] public string? Nom { get; set; }
    [Column("PRENOM")] public string? Prenom { get; set; }
    [Column("CODE_TYPE_MO")] public decimal? CodeMO { get; set; }
}