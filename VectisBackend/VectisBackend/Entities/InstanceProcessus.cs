using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("INSTANCEPROCESSUS")]
public class InstanceProcessus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID")]
    public int Id { get; set; }

    [Column("DESIGNATION"), StringLength(100)]
    public string? Designation { get; set; }

    [Column("IDPROCESSUS")]
    public int? IdProcessus { get; set; }

    [Column("STATUT", TypeName = "float")]
    public double? Statut { get; set; }

    [Column("IDACTEGESTION", TypeName = "numeric(18,0)")]
    public decimal? IdActeGestion { get; set; }
}