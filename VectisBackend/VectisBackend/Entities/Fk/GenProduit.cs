using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_PRODUIT")]
public class GenProduit
{
    [Key]
    [Column("CODE_PRODUIT")]
    public decimal CodeProduit { get; set; }

    [Column("PRODUIT_ID")] public string? ProduitId { get; set; }
    [Column("DESIGNATION")] public string? Designation { get; set; }
    [Column("CODE_UNITE")] public decimal? CodeUnite { get; set; }
    [Column("DATE_FERMETURE")] public DateTime? DateFermeture { get; set; }
    [Column("PRODUIT_GENERIQUE")] public int? ProduitGenerique { get; set; }
}