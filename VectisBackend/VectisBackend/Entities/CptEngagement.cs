using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("CPT_ENGAGEMENT")]
public class CptEngagement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_ENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal CodeEngagement { get; set; }

    [Column("ANNEE", TypeName = "numeric(38,0)")]
    public decimal? Annee { get; set; }

    [Column("NUMERO_ORDRE"), StringLength(4000)]
    public string? NumeroOrdre { get; set; }

    [Column("CODE_OBJETENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal? CodeObjetEngagement { get; set; }

    [Column("TYPE_CREDIT", TypeName = "numeric(38,0)")]
    public decimal? TypeCredit { get; set; }

    [Column("DATE_ENGAGEMENT")]
    public DateTime? DateEngagement { get; set; }

    [Column("ACCORDE", TypeName = "numeric(38,0)")]
    public decimal? Accorde { get; set; }

    [Column("date_operation_validation", TypeName = "date")]
    public DateTime? DateOperationValidation { get; set; }

    [Column("CODE_TYPE_ENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal? CodeTypeEngagement { get; set; }

    [Column("CODE_MODE_ENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal? CodeModeEngagement { get; set; }

    [Column("TYPE_ENGAGEMENT", TypeName = "numeric(38,0)")]
    public decimal? TypeEngagement { get; set; }
}