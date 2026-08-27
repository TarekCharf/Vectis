using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services;

public interface IEngagementMapperService
{
    CptEngagement MapToCptEngagement
        (EngagementRowDto row, decimal codeTypeCredit, decimal annee, string numOrdre, decimal codeTypeContract);

    CptEngagementLigne MapToCptEngagementLigne
        (EngagementRowDto row, decimal codeEngagement, decimal codeDepense, decimal codeLigne, decimal disponibleCP,
        decimal disponibleCE, decimal creditEngCP, decimal creditEngCE, decimal creditDef, decimal compteurCP);

}

