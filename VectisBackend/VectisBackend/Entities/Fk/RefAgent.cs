using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("REF_AGENT")]
public class RefAgent
{
    [Key]
    [Column("CODE_AGENT")]
    public decimal CodeAgent { get; set; }

    [Column("NOM")] public string? Nom { get; set; }
    [Column("PRENOM")] public string? Prenom { get; set; }
    [Column("MATRICULE")] public string? Matricule { get; set; }
    [Column("PERSONNE_HABILITEE")] public decimal? PersonneHabilitee { get; set; }
}