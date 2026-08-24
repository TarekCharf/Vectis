using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_VILLE")]
public class GenVille
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_VILLE")]

    public decimal CodeVille { get; set; }

    [Column("DESCRIPTION_VILLE")] public string? DescriptionVille { get; set; }

}
