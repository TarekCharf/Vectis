using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_FOURNISSEUR")]
public class GenFournisseur
{
    [Key]
    [Column("CODE_FRS")]
    public decimal CodeFrs { get; set; }

    [Column("LIBELLE_FRS")] public string? LibelleFrs { get; set; }
    [Column("TYPE_FRS")] public decimal? TypeFrs { get; set; }
}