using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("DEP_TYPE_CONTRAT")]
public class DepTypeContrat
{
    [Key]
    [Column("CODE_TYPE_CONTRAT")]
    public decimal CodeTypeContrat { get; set; }

    [Column("LIBELLE_TYPE_CONTRAT")] public string? LibelleTypeContrat { get; set; }
}