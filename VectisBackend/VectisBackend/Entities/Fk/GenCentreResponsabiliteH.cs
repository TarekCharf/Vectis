using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_CENTRE_RESPONSABILITE_H")]
public class GenCentreResponsabiliteH
{
    [Column("REF_UTIL")]public string? RefUtil { get; set; }
    [Column("CODE_FILS")] public string? CodeFils { get; set; }
    [Column("LIBELLE")] public string? Libelle { get; set; }
    [Column("REF")] public string? Ref { get; set; }
    [Column("TYPE")] public string? Type { get; set; }
}