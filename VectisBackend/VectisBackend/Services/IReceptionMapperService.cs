using VectisBackend.Entities;
using VectisBackend.Models.Import;

namespace VectisBackend.Services;

public interface IReceptionMapperService
{
    DepReception MapToDepReception(
        ReceptionRowDto row, decimal num, decimal codeMarche, decimal codeNature, decimal codeType);

    DepReceptionReleve MapToDepReceptionReleve(int codeReception, decimal codeReleve);

    InstanceProcessus MapToInstanceProcessus(decimal codeReception, int valide);
}
