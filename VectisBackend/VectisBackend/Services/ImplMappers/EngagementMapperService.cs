using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers;

public class EngagementMapperService : IEngagementMapperService
{
    public CptEngagement MapToCptEngagement
        (EngagementRowDto row, decimal codeTypeCredit, decimal annee, string numOrdre, decimal codeTypeContract)
    {
        decimal? codeObjEng = codeTypeContract;
        if (codeTypeContract == 1 || codeTypeContract == 4 || codeTypeContract == 5)
        {
            codeObjEng = 1;
        }

        return new CptEngagement
        {
            Annee = annee,
            NumeroOrdre = numOrdre,
            CodeObjetEngagement = codeObjEng,
            TypeCredit = 2,
            DateEngagement = row.DateEngagement,
            Accorde = ParseOuiNonNumeric(row.Validation),
            DateOperationValidation = row.DateValidation,
            CodeTypeEngagement = codeTypeCredit,
            CodeModeEngagement = 1,
            TypeEngagement = 22
        };
    }

    public CptEngagementLigne MapToCptEngagementLigne(
        EngagementRowDto row, decimal codeEngagement, decimal codeDepense, decimal codeLigne,
        decimal disponibleCP, decimal disponibleCE,
        decimal creditEngCP, decimal creditEngCE, decimal creditDef,
        decimal compteurCP)
    {
        return new CptEngagementLigne
        {
            CodeEngagement = codeEngagement,
            CodeDepense = codeDepense,
            CodeLigne = codeLigne,
            MontantEngageCp = row.MontantEngagerCP,
            MontantEngageCe = row.MontantEngagerCE,
            DisponibleCp = disponibleCP,
            DisponibleCe = disponibleCE,
            CreditEngageCp = creditEngCP,
            CreditEngageCe = creditEngCE,
            CreditDefinitifCp = creditDef,
            CreditDefinitifCe = disponibleCE,
            MontantMarche = 0,
            CompteurCp = compteurCP,
            CompteurCe = 0
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

