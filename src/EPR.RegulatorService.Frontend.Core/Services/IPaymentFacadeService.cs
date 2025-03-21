using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Services;

public interface IPaymentFacadeService
{
    Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request);

    Task<ProducerPaymentResponse?> GetProducerPaymentDetailsAsync(ProducerPaymentRequest request);

    Task<CompliancePaymentResponse?> GetCompliancePaymentDetailsAsync(CompliancePaymentRequest request);

    Task<PackagingProducerPaymentResponse?> GetProducerPaymentDetailsForResubmissionAsync(PackagingProducerPaymentRequest request);

    Task<PackagingCompliancePaymentResponse?> GetCompliancePaymentDetailsForResubmissionAsync(PackagingCompliancePaymentRequest request);
}