using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("ETAPEVALIDATION")]
public class EtapeValidation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID", TypeName = "int")]
    public int Id { get; set; }

    [Column("DESIGNATION"), StringLength(100)]
    public string Designation { get; set; } = "";

    [Column("IDPROCESSUS", TypeName = "int")]
    public int IdProcessus { get; set; }

    [Column("NIVEAU", TypeName = "numeric(18, 0)")]
    public decimal Niveau { get; set; }
}
