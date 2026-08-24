using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_VENTIL_QUANTITE_RELEVE")]
public class DepVentilQuantiteReleve
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_VENTIL_QUANTITE", TypeName = "numeric(10,0)")]
    public decimal CodeVentilQuantite { get; set; }

    [Column("CODE_CENTRE", TypeName = "numeric(12,0)")]
    public decimal? CodeCentre { get; set; }

    [Column("CODE_RELEVE", TypeName = "numeric(10,0)")]
    public decimal? CodeReleve { get; set; }

    [Column("CODE_FILS_PRODUIT"), StringLength(10)]
    public string? CodeFilsProduit { get; set; }

    [Column("QUANTITE_LIV", TypeName = "numeric(14,4)")]
    public decimal? QuantiteLiv { get; set; }

    [Column("QUANTITE_ACPT", TypeName = "numeric(14,4)")]
    public decimal? QuantiteAcpt { get; set; }
}