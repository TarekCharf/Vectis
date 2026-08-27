using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("INSTANCEETAPE")]
public class InstanceEtape
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID", TypeName = "bigint")]
    public long Id { get; set; }

    [Column("IDINSTANCEPROCESSUS")]
    public int IdInstanceProcessus { get; set; }

    [Column("DATEACTION", TypeName = "datetime")]
    public DateTime DateAction { get; set; }

    [Column("DATESYSTEME", TypeName = "datetime")]
    public DateTime DateSysteme { get; set; }

    [Column("STATUT", TypeName = "float")]
    public double Statut { get; set; }

    [Column("IDETAPE", TypeName = "int")]
    public int IdEtape { get; set; }

    [Column("IDUSERS", TypeName = "numeric(18, 0)")]
    public decimal IdUsers { get; set; }

    [Column("TYPEREJET", TypeName = "numeric(38, 0)")]
    public decimal TypeRejet { get; set; }

    [Column("VALIDATIONREJET", TypeName = "numeric(18, 0)")]
    public decimal ValidationRejet { get; set; }
}
