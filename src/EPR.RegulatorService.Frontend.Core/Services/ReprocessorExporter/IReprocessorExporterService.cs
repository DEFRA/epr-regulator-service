using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public interface IReprocessorExporterService
{
    Task<Registration> GetRegistrationByIdAsync(Guid id);
    Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(Guid id);
    Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(Guid registrationMaterialId);
    Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(Guid registrationId);
    Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(Guid registrationMaterialId);
    Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(Guid registrationMaterialId);
    Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(Guid registrationMaterialId);
    Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(Guid registrationMaterialId);

    Task MarkAsDulyMadeAsync(Guid registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest);
    Task SubmitOfflinePaymentAsync(OfflinePaymentRequest offlinePayment);
    Task UpdateRegistrationMaterialOutcomeAsync(Guid registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest);
    Task UpdateRegulatorRegistrationTaskStatusAsync(UpdateRegistrationTaskStatusRequest updateRegistrationTaskStatusRequest);
    Task UpdateRegulatorApplicationTaskStatusAsync(UpdateMaterialTaskStatusRequest updateMaterialTaskStatusRequest);
    Task<HttpResponseMessage> DownloadSamplingInspectionFile(FileDownloadRequest request);

    Task<Registration> GetRegistrationByIdWithAccreditationsAsync(Guid id, int? year = null);
    Task<AccreditationMaterialPaymentFees> GetPaymentFeesByAccreditationMaterialIdAsync(Guid accreditationMaterialId);
    Task SubmitAccreditationOfflinePaymentAsync(AccreditationOfflinePaymentRequest offlinePayment);
    Task MarkAccreditationAsDulyMadeAsync(Guid registrationMaterialId, AccreditationMarkAsDulyMadeRequest dulyMadeRequest);
}