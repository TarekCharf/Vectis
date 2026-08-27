using VectisBackend.Entities;

namespace VectisBackend.Services.ImplMappers;

public class ValidationMapperService : IValidationMapperService
{
    public InstanceProcessus MapToInstanceProcessus(decimal code, int idProcessus, int? valider)
    {
        return new InstanceProcessus
        {
            Designation = code.ToString(),
            IdProcessus = idProcessus,
            Statut = valider == 1 ? 1 : 4,
            IdActeGestion = idProcessus
        };
    }

    public InstanceEtape MaptoInstanceEtape(decimal id, decimal idEtape)
    {
        return new InstanceEtape
        {
            IdInstanceProcessus = (int)id,
            DateAction = DateTime.Now,
            DateSysteme = DateTime.Now,
            Statut = 1,
            IdEtape = (int)idEtape,
            IdUsers = 1,
            TypeRejet = 0,
            ValidationRejet = 0
        };
    }
}
