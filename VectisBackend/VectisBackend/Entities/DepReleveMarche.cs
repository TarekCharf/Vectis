using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_RELEVE_MARCHE")]
public class DepReleveMarche
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODERELEVE", TypeName = "numeric(10,0)")]
    public decimal CodeReleve { get; set; }

    [Column("NUMERO_ORDRE_RELEVE", TypeName = "numeric(10,0)")]
    public decimal NumeroOrdreReleve { get; set; }

    [Column("NUMERORELEVE"), StringLength(100)]
    public string NumeroReleve { get; set; } = default!;

    [Column("CODEMARCHE", TypeName = "numeric(10,0)")]
    public decimal CodeMarche { get; set; }

    [Column("REFERENCELIVRAISON"), StringLength(200)]
    public string? ReferenceLivraison { get; set; }

    [Column("Code_Responsable", TypeName = "numeric(18,0)")]
    public decimal? CodeResponsable { get; set; }

    [Column("CODE_MAGASIN", TypeName = "numeric(10,0)")]
    public decimal? CodeMagasin { get; set; }

    [Column("CODE_MAGASINIER", TypeName = "numeric(9,0)")]
    public decimal? CodeMagasinier { get; set; }

    [Column("DATERELEVE")]
    public DateTime? DateReleve { get; set; }

    [Column("DATEBONLIVRAISON")]
    public DateTime? DateBonLivraison { get; set; }

    [Column("Heure_Livraison"), StringLength(50)]
    public string? HeureLivraison { get; set; }

    [Column("DATERELEVEPREVIS", TypeName = "date")]
    public DateTime? DateRelevePrevis { get; set; }

    [Column("Date_Depot", TypeName = "date")]
    public DateTime? DateDepot { get; set; }

    [Column("RELEVE_VALIDE", TypeName = "numeric(38,0)")]
    public decimal? ReleveValide { get; set; }

    [Column("DATE_VALIDATION")]
    public DateTime? DateValidation { get; set; }

    [Column("ETABLI_PAR_MO", TypeName = "numeric(38,0)")]
    public decimal? EtabliParMo { get; set; }

    [Column("MTRELEVETTC", TypeName = "numeric(20,2)")]
    public decimal? MtReleveTtc { get; set; }

    [Column("MTRELEVEHT", TypeName = "numeric(20,2)")]
    public decimal? MtReleveHt { get; set; }

    [Column("ISRECEPTIONNED", TypeName = "numeric(38,0)")]
    public decimal? IsReceptionned { get; set; }

    [Column("ISFACTURED", TypeName = "numeric(10,0)")]
    public decimal? IsFactured { get; set; }
}