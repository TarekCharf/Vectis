using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Models;

[Table("JOURNAL")]
public class Journal
{
    [Column("UTILISATEUR")]
    [StringLength(100)]
    public string? Utilisateur { get; set; }

    [Column("MODULE")]
    [StringLength(100)]
    public string? Module { get; set; }

    [Column("OPERATION")]
    [StringLength(100)]
    public string? Operation { get; set; }

    [Column("DATEOP")]
    [StringLength(100)]
    public string? DateOp { get; set; }

    [Column("HEURE")]
    [StringLength(100)]
    public string? Heure { get; set; }

    [Column("POSTE")]
    [StringLength(100)]
    public string? Poste { get; set; }

    [Column("REQUESTE")]
    [StringLength(4000)]
    public string? Requeste { get; set; }

    [Column("DESCRIPT")]
    [StringLength(2000)]
    public string? Descript { get; set; }

    [Column("NUMFORM")]
    [StringLength(5)]
    public string? Numform { get; set; }
}