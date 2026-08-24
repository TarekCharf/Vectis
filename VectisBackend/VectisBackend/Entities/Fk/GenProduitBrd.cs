using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_PRODUIT_BRD_1")]
public class GenProduitBrd
{
    [Key]
    [Column("CODE_PRODUIT")]
    public decimal CodeProduit { get; set; }

    [Column("PRODUIT_ID")] public string? ProduitId { get; set; }
    [Column("DESIGNATION")] public string? Designation { get; set; }
    [Column("CODE_UNITE")] public decimal? CodeUnite { get; set; }
    [Column("PRESTATION_ARCHITECTE")] public decimal? PrestationArchitecte { get; set; }
}