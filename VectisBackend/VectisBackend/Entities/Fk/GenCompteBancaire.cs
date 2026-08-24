using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_COMPTEBANCAIRE")]
public class GenCompteBancaire
{
    [Key]
    [Column("CODE_CPT_BANCAIRE")]
    public decimal CodeCptBancaire { get; set; }

    [Column("NUM_COMPTE")] public string? NumCompte { get; set; }
    [Column("CODE_FRS")] public decimal CodeFrs { get; set; }
    [Column("CODE_BANQUE")] public decimal? CodeBanque { get; set; }
    [Column("AGENCE")] public string? Agence { get; set; }
    [Column("VILLE")] public string? Ville { get; set; }
}