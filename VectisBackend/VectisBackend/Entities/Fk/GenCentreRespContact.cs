using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_CENTRE_RESP_CONTACTS")]
public class GenCentreRespContact
{
    [Key]
    [Column("CODE_CONTACT")]
    public decimal CodeContact { get; set; }
}
