using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("CPT_TYPE_CREDIT_SUPP")]
public class CptTypeCreditSupp
{
    [Column("CODE_TYPE_CREDIT")] public decimal CodeTypeCredit { get; set; }
    [Column("DESIGNATION_TYPE_CREDIT")] public string DesignationTypeCredit { get; set; }
}

