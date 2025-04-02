using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public interface IRegistrationService
{
    Task<Registration> GetRegistrationByIdAsync(int id);

    Task<RegistrationMaterial> GetRegistrationMaterialAsync(int registrationMaterialId);

    Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, ApplicationStatus? status, string comments);
}