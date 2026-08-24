using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_CENTRE_RESPONSABILITE")]
public class GenCentreResponsabilite
{
    [Key]
    [Column("CODE_CENTRE")]
    public decimal CodeCentre { get; set; }

    [Column("CENTRE_ID")] public string CentreId { get; set; } = "";
    [Column("DESIGNATION")] public string? Designation { get; set; }
    [Column("ID_SECTION")] public string? IdSection { get; set; }
    [Column("BLOQUER")] public decimal? Bloquer {  get; set; }
}