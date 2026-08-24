using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_VENTIL_MNTS_FACTURES")]
public class DepVentilMntsFacture
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CODE_VENTIL", TypeName = "numeric(38,0)")]
    public decimal CodeVentil { get; set; }

    [Column("CODE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal CodeFacture { get; set; }

    [Column("CODE_PRODUIT", TypeName = "numeric(10,0)")]
    public decimal? CodeProduit { get; set; }

    [Column("REVISION_TTC", TypeName = "numeric(20,2)")]
    public decimal? RevisionTtc { get; set; }

    [Column("RABAIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? RabaisTtc { get; set; }

    [Column("TRAVAUX_MOIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? TravauxMoisTtc { get; set; }

    [Column("TRAVAUX_SUPP_TTC", TypeName = "numeric(20,2)")]
    public decimal? TravauxSuppTtc { get; set; }

    [Column("RETENUES_FORFAITAIRES_TTC", TypeName = "numeric(20,2)")]
    public decimal? RetenuesForfaitairesTtc { get; set; }

    [Column("PENALITES_FORFAITAIRES_TTC", TypeName = "numeric(20,2)")]
    public decimal? PenalitesForfaitairesTtc { get; set; }

    [Column("PENALITES_RETARD_TTC", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardTtc { get; set; }

    [Column("PENALITES_RTDPRDCAUTIONDEF_TTC", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRtdprdcautiondefTtc { get; set; }

    [Column("MONTANT_FACTURE_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureTtc { get; set; }

    [Column("MONTANT_FACTURE_TVA", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureTva { get; set; }

    [Column("MONTANT_FACTURE_RS", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureRs { get; set; }

    [Column("APPROVISIONNEMENT_TTC", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementTtc { get; set; }

    [Column("RECUP_AVANCE_TTC", TypeName = "numeric(20,2)")]
    public decimal? RecupAvanceTtc { get; set; }

    [Column("RETENUES_TTC", TypeName = "numeric(20,2)")]
    public decimal? RetenuesTtc { get; set; }

    [Column("MONTAN_PAYE_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontanPayeTtc { get; set; }

    [Column("CODE_CENTRE", TypeName = "numeric(12,0)")]
    public decimal? CodeCentre { get; set; }

    [Column("PENALITES_RETARD_TVA", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardTva { get; set; }

    [Column("PENALITES_RETARD_RS", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardRs { get; set; }

    [Column("CODE_FILS_PRODUIT"), StringLength(50)]
    public string? CodeFilsProduit { get; set; }

    [Column("APPROVISIONNEMENT_TVA", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementTva { get; set; }

    [Column("RETENUES_TVA", TypeName = "numeric(20,2)")]
    public decimal? RetenuesTva { get; set; }

    [Column("RECUP_AVANCE_HT", TypeName = "numeric(20,4)")]
    public decimal? RecupAvanceHt { get; set; }

    [Column("RECUP_AVANCE_TVA", TypeName = "numeric(20,4)")]
    public decimal? RecupAvanceTva { get; set; }

    [Column("MONTANT_RS_TVA", TypeName = "decimal(18,2)")]
    public decimal? MontantRsTva { get; set; }

    [Column("BASE_RS_TVA", TypeName = "decimal(18,2)")]
    public decimal? BaseRsTva { get; set; }
}