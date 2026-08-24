using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_ORDRESERVICE")]
public class DepOrdreService
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_ORDRESERVICE", TypeName = "numeric(10,0)")]
    public decimal CodeOrdreService { get; set; }

    [Column("NUMEROORDRE_ORDRESERVICE", TypeName = "numeric(10,0)")]
    public decimal? NumeroOrdreOrdreService { get; set; }

    [Column("NUMERO_ORDRESERVICE"), StringLength(50)]
    public string? NumeroOrdreService { get; set; }

    [Column("CODE_MARCHE", TypeName = "numeric(10,0)")]
    public decimal? CodeMarche { get; set; }

    [Column("CODE_TYPEORDRESERVICE", TypeName = "numeric(10,0)")]
    public decimal? CodeTypeOrdreService { get; set; }

    [Column("DATE_ORDRESERVICE")]
    public DateTime? DateOrdreService { get; set; }

    [Column("DATE_NOTIFORDRESERVICE")]
    public DateTime? DateNotifOrdreService { get; set; }

    [Column("DATE_EFFETORDRESERVICE")]
    public DateTime? DateEffetOrdreService { get; set; }

    [Column("ORDRE_NOTIFICATION", TypeName = "numeric(38,0)")]
    public decimal? OrdreNotification { get; set; }

    [Column("CODE_GROUPEMENT"), StringLength(100)]
    public string? CodeGroupement { get; set; }

    [Column("CODE_MOTIF_ARRET", TypeName = "numeric(38,0)")]
    public decimal? CodeMotifArret { get; set; }

    [Column("valide", TypeName = "decimal(18,0)")]
    public decimal? Valide { get; set; }
}