using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public interface IReprocessorExporterService
{
    Task<Registration> GetRegistrationByIdAsync(int id);
    Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(int registrationMaterialId);
    Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(int registrationId);
    
    Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest);
    Task UpdateRegulatorRegistrationTaskStatusAsync(UpdateRegistrationTaskStatusRequest updateRegistrationTaskStatusRequest);
    Task UpdateRegulatorApplicationTaskStatusAsync(UpdateMaterialTaskStatusRequest updateMaterialTaskStatusRequest);
}