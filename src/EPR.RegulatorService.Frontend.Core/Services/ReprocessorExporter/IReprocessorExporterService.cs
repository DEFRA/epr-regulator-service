using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public interface IReprocessorExporterService
{
    Task<Registration> GetRegistrationByIdAsync(int id);
    Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(int id);
    Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(int registrationMaterialId);
    Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(int registrationId);
    Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(int registrationMaterialId);
    Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(int registrationMaterialId);
    Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(int registrationMaterialId);
    Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(int registrationMaterialId);

    Task MarkAsDulyMadeAsync(int registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest);
    Task SubmitOfflinePaymentAsync(OfflinePaymentRequest offlinePayment);
    Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest);
    Task UpdateRegulatorRegistrationTaskStatusAsync(UpdateRegistrationTaskStatusRequest updateRegistrationTaskStatusRequest);
    Task UpdateRegulatorApplicationTaskStatusAsync(UpdateMaterialTaskStatusRequest updateMaterialTaskStatusRequest);


    //Get registrationByDate
    //This should return a registration
    Task<Registration> GetRegistrationByDateAsync(int id);
}