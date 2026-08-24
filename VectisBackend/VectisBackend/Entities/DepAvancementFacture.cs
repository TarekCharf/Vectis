using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_AVANCEMENT_FACTURE")]
public class DepAvancementFacture
{
    // Clé composite : (CODEAVANCEMENT, CODE_FACTURE, CODE_FILS_PRODUIT)
    // À configurer dans OnModelCreating
    [Column("CODEAVANCEMENT", TypeName = "numeric(10,0)")]
    public decimal CodeAvancement { get; set; }

    [Column("CODE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal CodeFacture { get; set; }

    [Column("CODE_FILS_PRODUIT"), StringLength(50)]
    public string CodeFilsProduit { get; set; } = null!;

    [Column("QTEEXECUTEE", TypeName = "numeric(20,2)")]
    public decimal? QteExecutee { get; set; }

    [Column("QTEACCEPTEE", TypeName = "numeric(20,2)")]
    public decimal? QteAcceptee { get; set; }
}