using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_MONTANTS_FACTURES")]
public class DepMontantsFacture
{
    [Key]
    [Column("CODE_FACTURE", TypeName = "numeric(10,0)")]
    public decimal CodeFacture { get; set; }

    [Column("CUMUL_ACTUEL_HT", TypeName = "numeric(20,4)")]
    public decimal? CumulActuelHt { get; set; }

    [Column("CUMUL_ACTUEL_TVA", TypeName = "numeric(20,4)")]
    public decimal? CumulActuelTva { get; set; }

    [Column("CUMUL_ACTUEL_TTC", TypeName = "numeric(20,4)")]
    public decimal? CumulActuelTtc { get; set; }

    [Column("REVISION_HT", TypeName = "numeric(20,4)")]
    public decimal? RevisionHt { get; set; }

    [Column("REVISION_TVA", TypeName = "numeric(20,4)")]
    public decimal? RevisionTva { get; set; }

    [Column("REVISION_TTC", TypeName = "numeric(20,4)")]
    public decimal? RevisionTtc { get; set; }

    [Column("RABAIS_HT", TypeName = "numeric(20,4)")]
    public decimal? RabaisHt { get; set; }

    [Column("RABAIS_TVA", TypeName = "numeric(20,4)")]
    public decimal? RabaisTva { get; set; }

    [Column("RABAIS_TTC", TypeName = "numeric(20,4)")]
    public decimal? RabaisTtc { get; set; }

    [Column("TRAVAUX_MOIS_HT", TypeName = "numeric(20,4)")]
    public decimal? TravauxMoisHt { get; set; }

    [Column("TRAVAUX_MOIS_TVA", TypeName = "numeric(20,4)")]
    public decimal? TravauxMoisTva { get; set; }

    [Column("TRAVAUX_MOIS_TTC", TypeName = "numeric(20,4)")]
    public decimal? TravauxMoisTtc { get; set; }

    [Column("TRAVAUX_SUPP_HT", TypeName = "numeric(20,4)")]
    public decimal? TravauxSuppHt { get; set; }

    [Column("TRAVAUX_SUPP_TVA", TypeName = "numeric(20,4)")]
    public decimal? TravauxSuppTva { get; set; }

    [Column("TRAVAUX_SUPP_TTC", TypeName = "numeric(20,4)")]
    public decimal? TravauxSuppTtc { get; set; }

    [Column("RETENUES_FORFAITAIRES_HT", TypeName = "numeric(20,4)")]
    public decimal? RetenuesForfaitairesHt { get; set; }

    [Column("RETENUES_FORFAITAIRES_TVA", TypeName = "numeric(20,4)")]
    public decimal? RetenuesForfaitairesTva { get; set; }

    [Column("RETENUES_FORFAITAIRES_TTC", TypeName = "numeric(20,4)")]
    public decimal? RetenuesForfaitairesTtc { get; set; }

    [Column("PENALITES_FORFAITAIRES_HT", TypeName = "numeric(20,4)")]
    public decimal? PenalitesForfaitairesHt { get; set; }

    [Column("PENALITES_FORFAITAIRES_TVA", TypeName = "numeric(20,4)")]
    public decimal? PenalitesForfaitairesTva { get; set; }

    [Column("PENALITES_FORFAITAIRES_TTC", TypeName = "numeric(20,4)")]
    public decimal? PenalitesForfaitairesTtc { get; set; }

    [Column("MONTANT_FACTURE_HT", TypeName = "numeric(20,4)")]
    public decimal? MontantFactureHt { get; set; }

    [Column("MONTANT_FACTURE_TVA", TypeName = "numeric(20,4)")]
    public decimal? MontantFactureTva { get; set; }

    [Column("MONTANT_FACTURE_TTC", TypeName = "numeric(20,4)")]
    public decimal? MontantFactureTtc { get; set; }

    [Column("APPROVISIONNEMENT_HT", TypeName = "numeric(20,4)")]
    public decimal? ApprovisionnementHt { get; set; }

    [Column("APPROVISIONNEMENT_TVA", TypeName = "numeric(20,4)")]
    public decimal? ApprovisionnementTva { get; set; }

    [Column("APPROVISIONNEMENT_TTC", TypeName = "numeric(20,4)")]
    public decimal? ApprovisionnementTtc { get; set; }

    [Column("RECUP_AVANCE_TTC", TypeName = "numeric(20,4)")]
    public decimal? RecupAvanceTtc { get; set; }

    [Column("RETENUES_TTC", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTtc { get; set; }

    [Column("MONTAN_PAYE_TTC", TypeName = "numeric(20,4)")]
    public decimal? MontanPayeTtc { get; set; }

    [Column("RETENUES_RS", TypeName = "numeric(20,4)")]
    public decimal? RetenuesRs { get; set; }

    [Column("RETENUES_HT", TypeName = "numeric(20,4)")]
    public decimal? RetenuesHt { get; set; }

    [Column("RETENUES_TVA", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTva { get; set; }

    [Column("MONTANT_FACTURE_RS", TypeName = "numeric(20,4)")]
    public decimal? MontantFactureRs { get; set; }

    [Column("MONTANT_FACTURE_RS_TVA", TypeName = "decimal(18,2)")]
    public decimal? MontantFactureRsTva { get; set; }

    [Column("MONTANT_FACTURE_BASE_RS_TVA", TypeName = "decimal(18,2)")]
    public decimal? MontantFactureBaseRsTva { get; set; }

    [Column("MT_FACTURE_HT_BASE_RS_IR_IS", TypeName = "decimal(18,2)")]
    public decimal? MtFactureHtBaseRsIrIs { get; set; }

    [Column("MONTANT_FACTURE_HT_BASE_RS_TVA", TypeName = "decimal(18,2)")]
    public decimal? MontantFactureHtBaseRsTva { get; set; }
}