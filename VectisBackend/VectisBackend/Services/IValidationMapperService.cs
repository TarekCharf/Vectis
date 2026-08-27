using Microsoft.EntityFrameworkCore;
using VectisBackend.Entities;

namespace VectisBackend.Services;

public interface IValidationMapperService
{
    InstanceProcessus MapToInstanceProcessus(decimal code, int idProcessus, int? valider);

    InstanceEtape MaptoInstanceEtape(decimal id, decimal idEtape);
}
