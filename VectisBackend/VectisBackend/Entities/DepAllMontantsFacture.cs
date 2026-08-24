using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities;

[Table("DEP_ALL_MONTANTS_FACTURES")]
public class DepAllMontantsFacture
{
    [Key]
    [Column("CODE_FACTURE", TypeName = "numeric(38,0)")]
    public decimal CodeFacture { get; set; }

    [Column("CUMUL_ACTUEL_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulActuelHt { get; set; }

    [Column("CUMUL_ACTUEL_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulActuelTva { get; set; }

    [Column("CUMUL_ACTUEL_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActuelTtc { get; set; }

    [Column("REVISION_HT", TypeName = "numeric(20,2)")]
    public decimal? RevisionHt { get; set; }

    [Column("REVISION_TVA", TypeName = "numeric(20,2)")]
    public decimal? RevisionTva { get; set; }

    [Column("REVISION_TTC", TypeName = "numeric(20,2)")]
    public decimal? RevisionTtc { get; set; }

    [Column("RABAIS_HT", TypeName = "numeric(20,2)")]
    public decimal? RabaisHt { get; set; }

    [Column("RABAIS_TVA", TypeName = "numeric(20,2)")]
    public decimal? RabaisTva { get; set; }

    [Column("RABAIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? RabaisTtc { get; set; }

    [Column("CUMUL_ACT_APRES_RABAIS_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulActApresRabaisHt { get; set; }

    [Column("CUMUL_ACT_APRES_RABAIS_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulActApresRabaisTva { get; set; }

    [Column("CUMUL_ACT_APRES_RABAIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActApresRabaisTtc { get; set; }

    [Column("CUMUL_PREC_APRES_RABAIS_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecApresRabaisHt { get; set; }

    [Column("CUMUL_PREC_APRES_RABAIS_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecApresRabaisTva { get; set; }

    [Column("CUMUL_PREC_APRES_RABAIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecApresRabaisTtc { get; set; }

    [Column("TRAVAUX_MOIS_HT", TypeName = "numeric(20,2)")]
    public decimal? TravauxMoisHt { get; set; }

    [Column("TRAVAUX_MOIS_TVA", TypeName = "numeric(20,2)")]
    public decimal? TravauxMoisTva { get; set; }

    [Column("TRAVAUX_MOIS_TTC", TypeName = "numeric(20,2)")]
    public decimal? TravauxMoisTtc { get; set; }

    [Column("CUMUL_ACT_TRAVAUX_SUPP_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulActTravauxSuppHt { get; set; }

    [Column("CUMUL_ACT_TRAVAUX_SUPP_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulActTravauxSuppTva { get; set; }

    [Column("CUMUL_ACT_TRAVAUX_SUPP_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActTravauxSuppTtc { get; set; }

    [Column("CUMUL_PREC_TRAVAUX_SUPP_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecTravauxSuppHt { get; set; }

    [Column("CUMUL_PREC_TRAVAUX_SUPP_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecTravauxSuppTva { get; set; }

    [Column("CUMUL_PREC_TRAVAUX_SUPP_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecTravauxSuppTtc { get; set; }

    [Column("TRAVAUX_SUPP_HT", TypeName = "numeric(20,2)")]
    public decimal? TravauxSuppHt { get; set; }

    [Column("TRAVAUX_SUPP_TVA", TypeName = "numeric(20,2)")]
    public decimal? TravauxSuppTva { get; set; }

    [Column("TRAVAUX_SUPP_TTC", TypeName = "numeric(20,2)")]
    public decimal? TravauxSuppTtc { get; set; }

    [Column("CUMUL_ACT_RETENUES_FORF_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulActRetenuesForfHt { get; set; }

    [Column("CUMUL_ACT_RETENUES_FORF_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulActRetenuesForfTva { get; set; }

    [Column("CUMUL_ACT_RETENUES_FORF_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActRetenuesForfTtc { get; set; }

    [Column("CUMUL_PREC_RETENUES_FORF_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRetenuesForfHt { get; set; }

    [Column("CUMUL_PREC_RETENUES_FORF_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRetenuesForfTva { get; set; }

    [Column("CUMUL_PREC_RETENUES_FORF_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRetenuesForfTtc { get; set; }

    [Column("RETENUES_FORFAITAIRES_HT", TypeName = "numeric(20,2)")]
    public decimal? RetenuesForfaitairesHt { get; set; }

    [Column("RETENUES_FORFAITAIRES_TVA", TypeName = "numeric(20,2)")]
    public decimal? RetenuesForfaitairesTva { get; set; }

    [Column("RETENUES_FORFAITAIRES_TTC", TypeName = "numeric(20,2)")]
    public decimal? RetenuesForfaitairesTtc { get; set; }

    [Column("CUMUL_ACT_PENALITES_FORF_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulActPenalitesForfHt { get; set; }

    [Column("CUMUL_ACT_PENALITES_FORF_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulActPenalitesForfTva { get; set; }

    [Column("CUMUL_ACT_PENALITES_FORF_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActPenalitesForfTtc { get; set; }

    [Column("CUMUL_PREC_PENALITES_FORF_HT", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecPenalitesForfHt { get; set; }

    [Column("CUMUL_PREC_PENALITES_FORF_TVA", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecPenalitesForfTva { get; set; }

    [Column("CUMUL_PREC_PENALITES_FORF_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecPenalitesForfTtc { get; set; }

    [Column("PENALITES_FORFAITAIRES_HT", TypeName = "numeric(20,2)")]
    public decimal? PenalitesForfaitairesHt { get; set; }

    [Column("PENALITES_FORFAITAIRES_TVA", TypeName = "numeric(20,2)")]
    public decimal? PenalitesForfaitairesTva { get; set; }

    [Column("PENALITES_FORFAITAIRES_TTC", TypeName = "numeric(20,2)")]
    public decimal? PenalitesForfaitairesTtc { get; set; }

    [Column("PENALITES_RETARD_HT", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardHt { get; set; }

    [Column("PENALITES_RETARD_TVA", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardTva { get; set; }

    [Column("PENALITES_RETARD_TTC", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardTtc { get; set; }

    [Column("MONTANT_FACTURE_HT", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureHt { get; set; }

    [Column("MONTANT_FACTURE_TVA", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureTva { get; set; }

    [Column("MONTANT_FACTURE_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureTtc { get; set; }

    [Column("APPROVISIONNEMENT_ACT_HT", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementActHt { get; set; }

    [Column("APPROVISIONNEMENT_ACT_TVA", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementActTva { get; set; }

    [Column("APPROVISIONNEMENT_ACT_TTC", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementActTtc { get; set; }

    [Column("APPROVISIONNEMENT_PREC_HT", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementPrecHt { get; set; }

    [Column("APPROVISIONNEMENT_PREC_TVA", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementPrecTva { get; set; }

    [Column("APPROVISIONNEMENT_PREC_TTC", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementPrecTtc { get; set; }

    [Column("APPROVISIONNEMENT_HT", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementHt { get; set; }

    [Column("APPROVISIONNEMENT_TVA", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementTva { get; set; }

    [Column("APPROVISIONNEMENT_TTC", TypeName = "numeric(20,2)")]
    public decimal? ApprovisionnementTtc { get; set; }

    [Column("CUMUL_ACT_RECUP_AVANCE_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActRecupAvanceTtc { get; set; }

    [Column("CUMUL_PREC_RECUP_AVANCE_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRecupAvanceTtc { get; set; }

    [Column("RECUP_AVANCE_TTC", TypeName = "numeric(20,2)")]
    public decimal? RecupAvanceTtc { get; set; }

    [Column("CUMUL_ACT_RETENUES_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulActRetenuesTtc { get; set; }

    [Column("CUMUL_PREC_RETENUES_TTC", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRetenuesTtc { get; set; }

    [Column("RETENUES_TTC", TypeName = "numeric(20,2)")]
    public decimal? RetenuesTtc { get; set; }

    [Column("MONTAN_PAYE_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontanPayeTtc { get; set; }

    [Column("PENALITES_RTDPRDCAUTIONDEF_HT", TypeName = "numeric(20,4)")]
    public decimal? PenalitesRtdprdcautiondefHt { get; set; }

    [Column("PENALITES_RTDPRDCAUTIONDEF_TVA", TypeName = "numeric(20,4)")]
    public decimal? PenalitesRtdprdcautiondefTva { get; set; }

    [Column("PENALITES_RTDPRDCAUTIONDEF_TTC", TypeName = "numeric(20,4)")]
    public decimal? PenalitesRtdprdcautiondefTtc { get; set; }

    [Column("MONTANT_FACTURE_RS", TypeName = "numeric(20,2)")]
    public decimal? MontantFactureRs { get; set; }

    [Column("CUMUL_ACTUEL_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulActuelRs { get; set; }

    [Column("TRAVAUX_MOIS_RS", TypeName = "numeric(20,2)")]
    public decimal? TravauxMoisRs { get; set; }

    [Column("MONTANT_REGULARISATION_TTC", TypeName = "numeric(20,2)")]
    public decimal? MontantRegularisationTtc { get; set; }

    [Column("MONTANT_REGULARISATION_RS", TypeName = "numeric(20,2)")]
    public decimal? MontantRegularisationRs { get; set; }

    [Column("MONTANT_REGULARISATION_TVA", TypeName = "numeric(20,2)")]
    public decimal? MontantRegularisationTva { get; set; }

    [Column("MONTANT_REGULARISATION_HT", TypeName = "numeric(20,2)")]
    public decimal? MontantRegularisationHt { get; set; }

    [Column("MONTANT_RESTITUTION", TypeName = "numeric(20,4)")]
    public decimal? MontantRestitution { get; set; }

    [Column("MONTANT_RECUP_RETENUE", TypeName = "numeric(20,4)")]
    public decimal? MontantRecupRetenue { get; set; }

    [Column("MONTANT_RELIQUAT", TypeName = "numeric(20,4)")]
    public decimal? MontantReliquat { get; set; }

    [Column("CUMUL_ACT_RECUP_AVANCE_HT", TypeName = "numeric(20,4)")]
    public decimal? CumulActRecupAvanceHt { get; set; }

    [Column("CUMUL_ACT_RECUP_AVANCE_TVA", TypeName = "numeric(20,4)")]
    public decimal? CumulActRecupAvanceTva { get; set; }

    [Column("CUMUL_PREC_RECUP_AVANCE_TVA", TypeName = "numeric(20,4)")]
    public decimal? CumulPrecRecupAvanceTva { get; set; }

    [Column("CUMUL_PREC_RECUP_AVANCE_HT", TypeName = "numeric(20,4)")]
    public decimal? CumulPrecRecupAvanceHt { get; set; }

    [Column("RECUP_AVANCE_HT", TypeName = "numeric(20,4)")]
    public decimal? RecupAvanceHt { get; set; }

    [Column("RECUP_AVANCE_TVA", TypeName = "numeric(20,4)")]
    public decimal? RecupAvanceTva { get; set; }

    [Column("RABAIS_RS", TypeName = "numeric(20,2)")]
    public decimal? RabaisRs { get; set; }

    [Column("CUMUL_ACT_APRES_RABAIS_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulActApresRabaisRs { get; set; }

    [Column("CUMUL_ACT_TRAVAUX_SUPP_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulActTravauxSuppRs { get; set; }

    [Column("CUMUL_ACT_RETENUES_FORF_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulActRetenuesForfRs { get; set; }

    [Column("RETENUES_FORFAITAIRES_RS", TypeName = "numeric(20,2)")]
    public decimal? RetenuesForfaitairesRs { get; set; }

    [Column("CUMUL_ACT_PENALITES_FORF_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulActPenalitesForfRs { get; set; }

    [Column("CUMUL_PREC_APRES_RABAIS_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecApresRabaisRs { get; set; }

    [Column("CUMUL_PREC_TRAVAUX_SUPP_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecTravauxSuppRs { get; set; }

    [Column("TRAVAUX_SUPP_RS", TypeName = "numeric(20,2)")]
    public decimal? TravauxSuppRs { get; set; }

    [Column("CUMUL_PREC_RETENUES_FORF_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecRetenuesForfRs { get; set; }

    [Column("CUMUL_PREC_PENALITES_FORF_RS", TypeName = "numeric(20,2)")]
    public decimal? CumulPrecPenalitesForfRs { get; set; }

    [Column("PENALITES_FORFAITAIRES_RS", TypeName = "numeric(20,2)")]
    public decimal? PenalitesForfaitairesRs { get; set; }

    [Column("PENALITES_RETARD_RS", TypeName = "numeric(20,2)")]
    public decimal? PenalitesRetardRs { get; set; }

    [Column("RETENUES_TRV_SUP_RS", TypeName = "numeric(20,2)")]
    public decimal? RetenuesTrvSupRs { get; set; }

    [Column("CUMUL_ACT_RETENUES_HT", TypeName = "numeric(20,4)")]
    public decimal? CumulActRetenuesHt { get; set; }

    [Column("CUMUL_ACT_RETENUES_RS", TypeName = "numeric(20,4)")]
    public decimal? CumulActRetenuesRs { get; set; }

    [Column("CUMUL_ACT_RETENUES_TVA", TypeName = "numeric(20,4)")]
    public decimal? CumulActRetenuesTva { get; set; }

    [Column("CUMUL_PREC_RETENUES_HT", TypeName = "numeric(20,4)")]
    public decimal? CumulPrecRetenuesHt { get; set; }

    [Column("CUMUL_PREC_RETENUES_RS", TypeName = "numeric(20,4)")]
    public decimal? CumulPrecRetenuesRs { get; set; }

    [Column("CUMUL_PREC_RETENUES_TVA", TypeName = "numeric(20,4)")]
    public decimal? CumulPrecRetenuesTva { get; set; }

    [Column("RETENUES_HT", TypeName = "numeric(20,4)")]
    public decimal? RetenuesHt { get; set; }

    [Column("RETENUES_RS", TypeName = "numeric(20,4)")]
    public decimal? RetenuesRs { get; set; }

    [Column("RETENUES_TVA", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTva { get; set; }

    [Column("RETENUES_TRVSUP_HT", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTrvsupHt { get; set; }

    [Column("RETENUES_TRVSUP_TVA", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTrvsupTva { get; set; }

    [Column("RETENUES_TRVSUP_TTC", TypeName = "numeric(20,4)")]
    public decimal? RetenuesTrvsupTtc { get; set; }

    [Column("RETENUES_TRVSUP_RS", TypeName = "numeric(20,2)")]
    public decimal? RetenuesTrvsupRs { get; set; }

    [Column("TRAVAUX_REGIE", TypeName = "numeric(18,0)")]
    public decimal? TravauxRegie { get; set; }

    [Column("TRAVAUX_REGIE_HT", TypeName = "numeric(18,0)")]
    public decimal? TravauxRegieHt { get; set; }

    [Column("TRAVAUX_REGIE_TVA", TypeName = "numeric(18,0)")]
    public decimal? TravauxRegieTva { get; set; }

    [Column("TRAVAUX_REGIE_RS", TypeName = "numeric(18,0)")]
    public decimal? TravauxRegieRs { get; set; }

    [Column("MONTANT_REFACTION", TypeName = "numeric(20,4)")]
    public decimal? MontantRefaction { get; set; }

    [Column("MONTANT_CAUTION_DEFINITIVE", TypeName = "numeric(20,2)")]
    public decimal? MontantCautionDefinitive { get; set; }

    [Column("MONTANT_CAUTION_DEFINITIVE_HT", TypeName = "numeric(20,2)")]
    public decimal? MontantCautionDefinitiveHt { get; set; }

    [Column("MONTANT_CAUTION_DEFINITIVE_TVA", TypeName = "numeric(20,2)")]
    public decimal? MontantCautionDefinitiveTva { get; set; }

    [Column("MONTANT_CAUTION_DEFINITIVE_RS", TypeName = "numeric(20,2)")]
    public decimal? MontantCautionDefinitiveRs { get; set; }

    [Column("PENALITE_REMISE_CAUTION", TypeName = "numeric(20,2)")]
    public decimal? PenaliteRemiseCaution { get; set; }

    [Column("MT_PENALITE_DEPASS", TypeName = "numeric(10,2)")]
    public decimal? MtPenaliteDepass { get; set; }

    [Column("Penalite_Special", TypeName = "numeric(10,2)")]
    public decimal? PenaliteSpecial { get; set; }

    [Column("cumul_redressement", TypeName = "numeric(20,2)")]
    public decimal? CumulRedressement { get; set; }

    [Column("MONTANT_FACTURE_RS_TVA", TypeName = "decimal(20,4)")]
    public decimal? MontantFactureRsTva { get; set; }

    [Column("MONTANT_FACTURE_BASE_RS_TVA", TypeName = "decimal(20,4)")]
    public decimal? MontantFactureBaseRsTva { get; set; }

    [Column("MT_FACTURE_HT_BASE_RS_IR_IS", TypeName = "decimal(20,4)")]
    public decimal? MtFactureHtBaseRsIrIs { get; set; }

    [Column("MONTANT_FACTURE_HT_BASE_RS_TVA", TypeName = "decimal(20,4)")]
    public decimal? MontantFactureHtBaseRsTva { get; set; }
}