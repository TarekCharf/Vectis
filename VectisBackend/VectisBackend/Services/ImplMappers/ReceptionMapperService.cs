using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services.ImplMappers;

public class ReceptionMapperService : IReceptionMapperService
{
    public DepReception MapToDepReception(ReceptionRowDto row, decimal num, decimal codeMarche, decimal codeNature, decimal codeType)
    {
        return new DepReception
        {
            NumeroOrdreReception = num,
            NumeroReception = num.ToString(),
            CodeMarche = codeMarche,
            CodeNatureReception = codeNature,
            CodeTypeReception = codeType,
            DateReception = row.DateReception,
            LieuxReception = row.Lieux ?? "",
            NumeroFicheReception = row.NumFicheReception ?? "",
            HeureReception = row.HeureReception ?? "",
            Valider = ParseOuiNonNumeric(row.Validation),
            CodePeriodeReception = 1,
            MontantLiberationCautiondef = 0,
            RapportExecution = 0,
            CodeRecepParProv = -1,
            RestitutionAutoRetenue = 0
        };
    }

    public DepReceptionReleve MapToDepReceptionReleve(int codeReception, decimal codeReleve)
    {
        return new DepReceptionReleve
        {
            CodeReception = codeReception,
            CodeReleve = codeReleve
        };
    }

    public InstanceProcessus MapToInstanceProcessus(decimal codeReception, int valide)
    {
        return new InstanceProcessus
        {
            Designation = codeReception.ToString(),
            IdProcessus = 51,
            Statut = valide == 1 ? 1 : 4,
            IdActeGestion = 51
        };
    }

    private static int? ParseOuiNonNumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var v = value.Trim().ToUpperInvariant();
        if (v == "OUI") return 1;
        if (v == "NON") return 0;
        return null;
    }
}
