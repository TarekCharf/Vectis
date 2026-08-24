using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("GEN_TARIF_FOURNISSEUR")]
public class GenTarifFournisseur
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_LIGNE", TypeName = "numeric(18,0)")]
    public decimal CodeLigne { get; set; }

    [Column("CODE_FRS", TypeName = "numeric(18,0)")]
    public decimal? CodeFrs { get; set; }

    [Column("CODE_PRODUIT", TypeName = "numeric(18,0)")]
    public decimal? CodeProduit { get; set; }

    [Column("PRIX_FRS", TypeName = "numeric(20,4)")]
    public decimal? PrixFrs { get; set; }

    [Column("TAUX_REMISE", TypeName = "numeric(20,4)")]
    public decimal? TauxRemise { get; set; }
}