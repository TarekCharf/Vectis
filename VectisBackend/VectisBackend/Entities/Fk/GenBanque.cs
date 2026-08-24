using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_BANQUE")]
public class GenBanque
{
    [Key]
    [Column("CODE_BANQUE")]
    public decimal CodeBanque { get; set; }

    [Column("DESIGNATION_BANQUE")] public string? DesignationBanque { get; set; }
    [Column("CLESIEGE")] public decimal? CleSiege {  get; set; }
    [Column("SIEGE")] public decimal? Siege { get; set; }
    [Column("CODE_VILLE")] public decimal? CodeVille { get; set; }
}