using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_RECEPTION")]
public class DepReception
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_RECEPTION", TypeName = "numeric(38,0)")]
    public decimal CodeReception { get; set; }

    [Column("NUMEROORDRE_RECEPTION", TypeName = "numeric(38,0)")]
    public decimal? NumeroOrdreReception { get; set; }

    [Column("NUMERO_RECEPTION"), StringLength(100)]
    public string? NumeroReception { get; set; }

    [Column("CODE_MARCHE", TypeName = "numeric(10,0)")]
    public decimal? CodeMarche { get; set; }

    [Column("CODE_NATURERECEPTION", TypeName = "numeric(38,0)")]
    public decimal? CodeNatureReception { get; set; }

    [Column("CODE_TYPERECEPTION", TypeName = "numeric(10,0)")]
    public decimal? CodeTypeReception { get; set; }

    [Column("DATE_RECEPTION")]
    public DateTime? DateReception { get; set; }

    [Column("LIEUX_RECEPTION"), StringLength(250)]
    public string? LieuxReception { get; set; }

    [Column("Numero_Fiche_Reception"), StringLength(50)]
    public string? NumeroFicheReception { get; set; }

    [Column("HEURE_RECEPTION"), StringLength(30)]
    public string? HeureReception { get; set; }

    [Column("VALIDER")]
    public int? Valider { get; set; }

    [Column("CODE_PERIODE_RECEPTION", TypeName = "decimal(10,0)")]
    public decimal? CodePeriodeReception { get; set; }

    [Column("MONTANT_LIBERATION_CAUTIONDEF", TypeName = "decimal(20,2)")]
    public decimal? MontantLiberationCautiondef { get; set; }

    [Column("RAPPORT_EXECUTION", TypeName = "decimal(1,0)")]
    public decimal? RapportExecution { get; set; }

    [Column("code_Recep_ParProv", TypeName = "decimal(10,0)")]
    public decimal? CodeRecepParProv { get; set; }

    [Column("Restitution_Auto_Retenue", TypeName = "decimal(10,0)")]
    public decimal? RestitutionAutoRetenue { get; set; }
}