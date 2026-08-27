using System.Globalization;
using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers
{
    public class FicheGeneraleMapperService : IFicheGeneraleMapperService
    {
        public DepMarche MapFicheGeneraleToDepMarche(
            FicheGeneraleRowDto row,
            decimal codeTypeContrat,
            decimal codeFournisseur,
            decimal codeDevise,
            decimal? codeTypeBudget,
            int? codeTypePrestation,
            decimal? codeNaturePrestation,
            decimal? codeCentreGes,
            decimal? codeMo,
            decimal? codeModePaiement,
            decimal? codeBanquePaiement,
            decimal? personneHabilitee,
            decimal? delaiExecution,
            decimal? delaiGarantie,
            decimal? uniteDelaiExecution,
            decimal? uniteDelaiGarantie)
        {
            var marche = new DepMarche
            {
                CodeTypeContrat = codeTypeContrat, // ex: marché/BC/CDC
                Exercice = row.Exercice,
                NumeroMarche = row.NumeroDepense,
                CodeTypePrestation = codeTypePrestation,
                NaturePrestation = codeNaturePrestation,
                CodeTypeBudget = codeTypeBudget ?? 0,
                LibelleMarche = row.Objet,
                ObservationMarche = row.Observation,
                CodeFournisseur = codeFournisseur,
                Representant = row.Representant,
                CodeCentreGes = codeCentreGes,
                CodeMO = codeMo,
                DateNotification = DateHelper.Normalize(row.DateDepense),
                CodeDevise = codeDevise,
                DelaiExecution = delaiExecution,
                DelaiGarantie = delaiGarantie,
                UniteDelaiExecution = uniteDelaiExecution,
                UniteDelaiGarantie = uniteDelaiGarantie,
                CodeModePaiement = codeModePaiement,
                CodeBanquePaiement = codeBanquePaiement,
                NumComptePaiement = row.NumeroCompte,
                PersonneHabilitee = personneHabilitee,
                TauxRetenueSource = row.TauxRetenueSource ?? 0,
                TauxRabais = row.TauxRemise ?? 0,
                Valide = ParseOuiNonNumeric(row.Validation), // Oui => 1, Non => 0
                DateAchevement = DateHelper.Normalize(ComputeDateAchevement(row.DateDepense, delaiExecution, uniteDelaiExecution)),

                // Valeurs calculées/remplies plus tard par agrégation bordereau
                TauxTva = 0,
                MontantMarcheHt = 0,
                MontantMarcheTtc = 0,
                MtMarcheRabais = 0,
            };

            return marche;
        }

        public DepMarcheCentre MapToDepMarcheCentre(decimal codeMarche, decimal codeCentreBeneficiaire)
        {
            return new DepMarcheCentre
            {
                CodeMarche = codeMarche,
                CodeCentre = codeCentreBeneficiaire
            };
        }

        public DepChargeSuivi? MapToDepChargeSuivi(FicheGeneraleRowDto row, decimal codeMarche, decimal? codeAgentChargeSuivi)
        {
            if (string.IsNullOrWhiteSpace(row.ChargeSuiviAgent) && string.IsNullOrWhiteSpace(row.ChargeSuiviMission))
                return null;

            return new DepChargeSuivi
            {
                CodeMarche = codeMarche,
                CodeAgent = codeAgentChargeSuivi,
                Mission = row.ChargeSuiviMission
            };
        }

        public DepMarchesH MapDepenseNodeToDepMarchesH(
            string codeFils, string libelleNumeroDepense, string codePere, string codeH,
            decimal nbProduits, string typeDepense, decimal codeMarche)
        {
            return new DepMarchesH
            {
                RefUtil = null, // à remplir selon ta règle "dernier + 1"
                CodeFils = codeFils,
                Libelle = libelleNumeroDepense,
                CodePere = codePere, // "00000000" selon ton doc
                Ref = codeMarche.ToString(),
                Type = typeDepense, // ex: "Marché/BC/CDC"
                Selection = 0,
                Niv = 2,
                CodeH = codeH, // ex: "1.001"
                NbF = nbProduits,
                ChOuRec = 0
            };
        }

        public DepEtapeMarche MapToDepEtapeMarche(FicheGeneraleRowDto row, decimal codeEtape, decimal codeMarche, decimal nEtape)
        {
            return new DepEtapeMarche
            {
                CodeEtape = codeEtape,
                CodeMarche = codeMarche,
                Selectionner = "1",
                DateEtape = row.DateDepense,
                DateEtapePrevisionnelle = row.DateDepense,
                Reference = "",
                Netape = nEtape
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

        private static DateTime? ComputeDateAchevement(DateTime? dateDepense, decimal? delaiExecution, decimal? uniteDelaiExecution)
        {
            if (!dateDepense.HasValue || !delaiExecution.HasValue) return null;

            switch (uniteDelaiExecution)
            {
                case 1: // jours
                    return dateDepense.Value.AddDays((double)delaiExecution.Value);
                case 2: // mois
                    return dateDepense.Value.AddMonths((int)delaiExecution.Value);
                case 3: // semestre
                    return dateDepense.Value.AddMonths((int)delaiExecution.Value * 6);
                case 4: // trimestre
                    return dateDepense.Value.AddMonths((int)delaiExecution.Value * 3);
                case 5: // année
                    return dateDepense.Value.AddYears((int)delaiExecution.Value);
                default: return null;
            }
        }

        public static class DateHelper
        {
            public static DateTime? Normalize(DateTime? date)
            {
                if (!date.HasValue)
                    return null;

                // Si la date est 0001-01-01 ou trop ancienne
                if (date.Value.Year < 1900)
                    return null;

                // Si la date est dans le futur (au-delà de 2100)
                if (date.Value.Year > 2100)
                    return null;

                return date.Value;
            }

            public static DateTime? ParseExcelDate(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                // Si c'est un nombre (date Excel)
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var excelDate))
                {
                    if (excelDate > 0)
                        return DateTime.FromOADate(excelDate);
                }

                // Essayer les formats de date
                var formats = new[] {
                    "dd/MM/yyyy",
                    "MM/dd/yyyy",
                    "yyyy-MM-dd",
                    "dd-MM-yyyy",
                    "dd/MM/yyyy HH:mm:ss",
                    "MM/dd/yyyy HH:mm:ss"
                };

                if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }

                return null;
            }
        }
    }
}
