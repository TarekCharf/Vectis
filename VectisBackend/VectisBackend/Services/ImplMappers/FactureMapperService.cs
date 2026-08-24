using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers;

public class FactureMapperService : IFactureMapperService
{
    public DepFacture MapToDepFacture(FactureRowDto row, decimal numOrdreFacture, string numeroFacture, decimal codeMarche, decimal? periode, decimal? codeFournisseur, decimal? montantHT, decimal? montantTTC)
    {
        return new DepFacture
        {
            NumeroOrdreFacture = numOrdreFacture,
            NumeroFacture = numeroFacture,
            CodeFournisseur = codeFournisseur,
            CodeMarche = codeMarche,
            ReferenceFacture = row.NumeroFactureFournisseur,
            DateFacture = row.DateFacture,
            DateArrive = row.DateDepotFacture,
            Mois = periode,
            CodeTypeContrat = 1,
            TypeFacture = 2,
            MontantRegularisationTtc = 0,
            MontantRegularisationRs = 0,
            MontantRegularisationTva = 0,
            MontantRegularisationHt = 0,
            MontantFactureFrsTtc = montantTTC,
            MontantFactureFrsHt = montantHT,
            CalculRgtotal = ParseOuiNonNumeric(row.CalculerRgTotal),
            CalculPenaltes = ParseOuiNonNumeric(row.CalculerPenalitesRetard),
            TauxRsSurTvaApplique = (int?)(row.TauxRsSurTvaApplique),
            PenaliteSpe = ParseOuiNonNumeric(row.PenaliteSpeciale),
            NbreJrMnq = row.PenaliteSpecialeNbreJoursManquement ?? 0,
            Indemnisation = row.PenaliteSpecialeIndemnisation ?? 0,
            ObservationServiceFait = row.Observation ?? "",
            Justification = row.Justification ?? "",
            Valider = ParseOuiNonNumeric(row.Validation),
            DateValidation = row.DateValidation,
            Ordonnoncee = 0
        };
    }

    public DepFactureReleve MapToDepFactureReleve(decimal codeFacture, decimal codeReleve)
    {
        return new DepFactureReleve
        {
            CodeFacture = codeFacture,
            CodeReleve = codeReleve
        };
    }

    public DepAvancementFacture MapToDepAvancementFacture(decimal codeAvancement, decimal codeFacture, string codeFilsProduit, decimal quantiteLivre, decimal quantiteAcceptee)
    {
        return new DepAvancementFacture
        {
            CodeAvancement = codeAvancement,
            CodeFacture = codeFacture,
            CodeFilsProduit = codeFilsProduit,
            QteExecutee = quantiteLivre,
            QteAcceptee = quantiteAcceptee
        };
    }

    public DepVentilMntsFacture MapToDepVentilMntsFacture(FactureRowDto row, decimal codeFacture, decimal codeProduit, decimal remiseProduit, decimal montantProduitHT, string codeFilsProduit, decimal? codeCentre, decimal? tauxRs, decimal tauxTva)
    {
        return new DepVentilMntsFacture
        {
            CodeFacture = codeFacture,
            CodeProduit = codeProduit,
            RevisionTtc = row.MontantRevision ?? 0,
            RabaisTtc = remiseProduit,
            TravauxMoisTtc = montantProduitHT + montantProduitHT * (tauxTva / 100m),
            TravauxSuppTtc = 0,
            RetenuesForfaitairesTtc = 0,
            PenalitesForfaitairesTtc = 0,
            PenalitesRetardTtc = 0,
            PenalitesRtdprdcautiondefTtc = 0,
            MontantFactureTtc = montantProduitHT + montantProduitHT * (tauxTva / 100m) + (row.MontantRevision ?? 0),
            MontantFactureTva = montantProduitHT * (tauxTva / 100m),
            MontantFactureRs = -(montantProduitHT * (tauxTva / 100m)) * (tauxRs / 100m),
            ApprovisionnementTtc = 0,
            RecupAvanceTtc = 0,
            RetenuesTtc = 0,
            MontanPayeTtc = montantProduitHT + montantProduitHT * (tauxTva / 100m) + (row.MontantRevision ?? 0) + (row.PenaliteSpecialeIndemnisation ?? 0),
            CodeCentre = codeCentre,
            PenalitesRetardTva = 0,
            PenalitesRetardRs = 0,
            CodeFilsProduit = codeFilsProduit,
            ApprovisionnementTva = 0,
            RetenuesTva = 0,
            RecupAvanceHt = 0,
            RecupAvanceTva = 0,
            MontantRsTva = 0,
            BaseRsTva = montantProduitHT * (tauxTva / 100m)
        };
    }

    public DepMontantsFacture MapToDepMontantsFacture(FactureRowDto row, decimal codeFacture)
    {
        return new DepMontantsFacture
        {
            CodeFacture = codeFacture,
            CumulActuelHt = 0,
            CumulActuelTva = 0,
            CumulActuelTtc = 0,
            RevisionHt = row.MontantRevision ?? 0,
            RevisionTva = 0,
            RevisionTtc = row.MontantRevision ?? 0,
            RabaisHt = 0,
            RabaisTva = 0,
            RabaisTtc = 0,
            TravauxMoisHt = row.MontantRevision ?? 0,
            TravauxMoisTva = 0,
            TravauxMoisTtc = row.MontantRevision ?? 0,
            TravauxSuppHt = 0,
            TravauxSuppTva = 0,
            TravauxSuppTtc = 0,
            RetenuesForfaitairesHt = 0,
            RetenuesForfaitairesTva = 0,
            RetenuesForfaitairesTtc = 0,
            PenalitesForfaitairesHt = 0,
            PenalitesForfaitairesTva = 0,
            PenalitesForfaitairesTtc = 0,
            MontantFactureHt = row.MontantRevision ?? 0,
            MontantFactureTva = 0,
            MontantFactureTtc = row.MontantRevision ?? 0,
            ApprovisionnementHt = 0,
            ApprovisionnementTva = 0,
            ApprovisionnementTtc = 0,
            RecupAvanceTtc = 0,
            RetenuesTtc = 0,
            MontanPayeTtc = (row.MontantRevision ?? 0) + (row.PenaliteSpecialeIndemnisation ?? 0),
            RetenuesRs = 0,
            RetenuesHt = 0,
            RetenuesTva = 0,
            MontantFactureRs = 0,
            MontantFactureRsTva = 0,
            MontantFactureBaseRsTva = 0,
            MtFactureHtBaseRsIrIs = row.MontantRevision ?? 0,
            MontantFactureHtBaseRsTva = row.MontantRevision ?? 0
        };
    }

    public DepAllMontantsFacture MapToDepAllMontantsFacture(FactureRowDto row, decimal codeFacture)
    {
        return new DepAllMontantsFacture
        {
            CodeFacture = codeFacture,
            CumulActuelHt = 0,
            CumulActuelTva = 0,
            CumulActuelTtc = 0,
            RevisionHt = row.MontantRevision ?? 0,
            RevisionTva = 0,
            RevisionTtc = row.MontantRevision ?? 0,
            RabaisHt = 0,
            RabaisTva = 0,
            RabaisTtc = 0,
            CumulActApresRabaisHt = row.MontantRevision ?? 0,
            CumulActApresRabaisTva = 0,
            CumulActApresRabaisTtc = row.MontantRevision ?? 0,
            CumulPrecApresRabaisHt = 0,
            CumulPrecApresRabaisTva = 0,
            CumulPrecApresRabaisTtc = 0,
            TravauxMoisHt = row.MontantRevision ?? 0,
            TravauxMoisTva = 0,
            TravauxMoisTtc = row.MontantRevision ?? 0,
            CumulActTravauxSuppHt = 0,
            CumulActTravauxSuppTva = 0,
            CumulActTravauxSuppTtc = 0,
            CumulPrecTravauxSuppHt = 0,
            CumulPrecTravauxSuppTva = 0,
            CumulPrecTravauxSuppTtc = 0,
            TravauxSuppHt = 0,
            TravauxSuppTva = 0,
            TravauxSuppTtc = 0,
            CumulActRetenuesForfHt = 0,
            CumulActRetenuesForfTva = 0,
            CumulActRetenuesForfTtc = 0,
            CumulPrecRetenuesForfHt = 0,
            CumulPrecRetenuesForfTva = 0,
            CumulPrecRetenuesForfTtc = 0,
            RetenuesForfaitairesHt = 0,
            RetenuesForfaitairesTva = 0,
            RetenuesForfaitairesTtc = 0,
            CumulActPenalitesForfHt = 0,
            CumulActPenalitesForfTva = 0,
            CumulActPenalitesForfTtc = 0,
            CumulPrecPenalitesForfHt = 0,
            CumulPrecPenalitesForfTva = 0,
            CumulPrecPenalitesForfTtc = 0,
            PenalitesForfaitairesHt = 0,
            PenalitesForfaitairesTva = 0,
            PenalitesForfaitairesTtc = 0,
            PenalitesRetardHt = 0,
            PenalitesRetardTva = 0,
            PenalitesRetardTtc = 0,
            MontantFactureHt = row.MontantRevision ?? 0,
            MontantFactureTva = 0,
            MontantFactureTtc = row.MontantRevision ?? 0,
            ApprovisionnementActHt = 0,
            ApprovisionnementActTva = 0,
            ApprovisionnementActTtc = 0,
            ApprovisionnementPrecHt = 0,
            ApprovisionnementPrecTva = 0,
            ApprovisionnementPrecTtc = 0,
            ApprovisionnementHt = 0,
            ApprovisionnementTva = 0,
            ApprovisionnementTtc = 0,
            CumulActRecupAvanceTtc = 0,
            CumulPrecRecupAvanceTtc = 0,
            RecupAvanceTtc = 0,
            CumulActRetenuesTtc = 0,
            CumulPrecRetenuesTtc = 0,
            RetenuesTtc = 0,
            MontanPayeTtc = (row.MontantRevision ?? 0) + (row.PenaliteSpecialeIndemnisation ?? 0),
            PenalitesRtdprdcautiondefHt = 0,
            PenalitesRtdprdcautiondefTva = 0,
            PenalitesRtdprdcautiondefTtc = 0,
            MontantFactureRs = 0,
            CumulActuelRs = 0,
            TravauxMoisRs = 0,
            MontantRegularisationTtc = 0,
            MontantRegularisationRs = 0,
            MontantRegularisationTva = 0,
            MontantRegularisationHt = 0,
            MontantRestitution = 0,
            MontantRecupRetenue = 0,
            MontantReliquat = 0,
            CumulActRecupAvanceHt = 0,
            CumulActRecupAvanceTva = 0,
            CumulPrecRecupAvanceTva = 0,
            CumulPrecRecupAvanceHt = 0,
            RecupAvanceHt = 0,
            RecupAvanceTva = 0,
            RabaisRs = 0,
            CumulActApresRabaisRs = 0,
            CumulActTravauxSuppRs = 0,
            CumulActRetenuesForfRs = 0,
            RetenuesForfaitairesRs = 0,
            CumulActPenalitesForfRs = 0,
            CumulPrecApresRabaisRs = 0,
            CumulPrecTravauxSuppRs = 0,
            TravauxSuppRs = 0,
            CumulPrecRetenuesForfRs = 0,
            CumulPrecPenalitesForfRs = 0,
            PenalitesForfaitairesRs = 0,
            PenalitesRetardRs = 0,
            RetenuesTrvSupRs = 0,
            CumulActRetenuesHt = 0,
            CumulActRetenuesRs = 0,
            CumulActRetenuesTva = 0,
            CumulPrecRetenuesHt = 0,
            CumulPrecRetenuesRs = 0,
            CumulPrecRetenuesTva = 0,
            RetenuesHt = 0,
            RetenuesRs = 0,
            RetenuesTva = 0,
            RetenuesTrvsupHt = 0,
            RetenuesTrvsupTva = 0,
            RetenuesTrvsupTtc = 0,
            RetenuesTrvsupRs = 0,
            TravauxRegie = 0,
            TravauxRegieHt = 0,
            TravauxRegieTva = 0,
            TravauxRegieRs = 0,
            MontantRefaction = 0,
            MontantCautionDefinitive = 0,
            MontantCautionDefinitiveHt = 0,
            MontantCautionDefinitiveTva = 0,
            MontantCautionDefinitiveRs = 0,
            PenaliteRemiseCaution = 0,
            MtPenaliteDepass = 0,
            PenaliteSpecial = 0,
            CumulRedressement = 0,
            MontantFactureRsTva = 0,
            MontantFactureBaseRsTva = 0,
            MtFactureHtBaseRsIrIs = row.MontantRevision ?? 0,
            MontantFactureHtBaseRsTva = row.MontantRevision ?? 0
        };
    }

    private static decimal? ParseOuiNonNumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var v = value.Trim().ToUpperInvariant();
        if (v == "OUI") return 1;
        if (v == "NON") return 0;
        return null;
    }
}
