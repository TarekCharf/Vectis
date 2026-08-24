using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("GEN_TYPE_REGLEMENT")]
public class GenTypeReglement
{
    [Key]
    [Column("CODE_TYPE_REGLEMENT")]
    public decimal CodeTypeReglement { get; set; }

    [Column("LIBELLE_TYPE_REGLEMENT")] public string? LibelleTypeReglement { get; set; }
}