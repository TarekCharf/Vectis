using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_LIGNE_RELEVEMARCHE")]
public class DepLigneReleveMarche
{
    [Column("CODERELEVE", TypeName = "numeric(10,0)")]
    public decimal CodeReleve { get; set; }

    [Column("CODE_FILS_PRODUIT"), StringLength(50)]
    public string CodeFilsProduit { get; set; } = default!;

    [Column("TAUX_TVA", TypeName = "numeric(10,4)")]
    public decimal? TauxTva { get; set; }

    [Column("POURC_LIVRAISON", TypeName = "float")]
    public double? PourcLivraison { get; set; }

    [Column("QTEEXECUTION", TypeName = "numeric(20,2)")]
    public decimal? QteExecution { get; set; }

    [Column("QTELIV", TypeName = "numeric(20,2)")]
    public decimal? QteLiv { get; set; }

    [Column("Qte_Ajournee", TypeName = "numeric(20,2)")]
    public decimal? QteAjournee { get; set; }

    [Column("TAUX_REFACTION", TypeName = "numeric(18,0)")]
    public decimal? TauxRefaction { get; set; }

    [Column("Qte_Refaction", TypeName = "numeric(18,0)")]
    public decimal? QteRefaction { get; set; }

    [Column("Controle")]
    public int? Controle { get; set; }

    [Column("Qte_Retourne", TypeName = "numeric(18,0)")]
    public decimal? QteRetourne { get; set; }
}