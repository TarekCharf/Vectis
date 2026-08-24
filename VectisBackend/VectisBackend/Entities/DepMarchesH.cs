using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_MARCHES_H")]
public class DepMarchesH
{
    [Key]
    [Column("CODE_FILS"), StringLength(50)]
    public string CodeFils { get; set; } = default!;

    [Column("REF_UTIL"), StringLength(50)]
    public string? RefUtil { get; set; }

    [Column("LIBELLE"), StringLength(500)]
    public string? Libelle { get; set; }

    [Column("CODE_PERE"), StringLength(50)]
    public string? CodePere { get; set; }

    [Column("REF"), StringLength(50)]
    public string? Ref { get; set; }

    [Column("TYPE"), StringLength(50)]
    public string? Type { get; set; }

    [Column("SELECTION", TypeName = "numeric(2,0)")]
    public decimal Selection { get; set; }

    [Column("NIV", TypeName = "numeric(10,0)")]
    public decimal? Niv { get; set; }

    [Column("CODE_H"), StringLength(50)]
    public string? CodeH { get; set; }

    [Column("NB_F", TypeName = "numeric(10,0)")]
    public decimal? NbF { get; set; }

    [Column("CH_OU_REC", TypeName = "numeric(10,0)")]
    public decimal? ChOuRec { get; set; }

    // Champs ajoutés depuis le second bloc SQL (Borderaux de prix)
    [Column("QUANTITE", TypeName = "numeric(10,2)")]
    public decimal? Quantite { get; set; }

    [Column("PRIX_MIN", TypeName = "numeric(20,4)")]
    public decimal? PrixMin { get; set; }

    [Column("PRIX_UNIT", TypeName = "numeric(19,4)")]
    public decimal? PrixUnit { get; set; }

    [Column("UNITE"), StringLength(50)]
    public string? Unite { get; set; }

    [Column("REF_UTIL_FORMAT"), StringLength(50)]
    public string? RefUtilFormat { get; set; }

    [Column("REFERENCE_PRODUIT", TypeName = "numeric(10,0)")]
    public decimal? ReferenceProduit { get; set; }

    [Column("TAUX_TVA", TypeName = "numeric(10,4)")]
    public decimal? TauxTva { get; set; }

    [Column("PRIX_TTC", TypeName = "numeric(19,4)")]
    public decimal? PrixTtc { get; set; }

    [Column("CARACT_LIGNE"), StringLength(2000)]
    public string? CaractLigne { get; set; }

    [Column("REF_TRI"), StringLength(50)]
    public string? RefTri { get; set; }

    [Column("HDDTVA", TypeName = "numeric(1,0)")]
    public decimal? Hddtva { get; set; }

    [Column("CALCUL_FRAIS_APPROCHE"), StringLength(1)]
    public string? CalculFraisApproche { get; set; }

    [Column("QUANTITE_MAX", TypeName = "numeric(20,2)")]
    public decimal? QuantiteMax { get; set; }

    [Column("REMISE"), StringLength(1)]
    public string? Remise { get; set; }

    [Column("PRIX_REVISABLE", TypeName = "numeric(18,0)")]
    public decimal? PrixRevisable { get; set; }

    [Column("applique_RS", TypeName = "numeric(1,0)")]
    public decimal? AppliqueRs { get; set; }

    [Column("applique_RS_TVA", TypeName = "numeric(1,0)")]
    public decimal? AppliqueRsTva { get; set; }
}