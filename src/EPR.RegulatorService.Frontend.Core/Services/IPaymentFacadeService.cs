using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Core.Services;

public interface IPaymentFacadeService
{
    Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request);

    Task<ProducerPaymentResponse?> GetProducerPaymentDetailsAsync(ProducerPaymentRequest request);

    Task<CompliancePaymentResponse?> GetCompliancePaymentDetailsAsync(CompliancePaymentRequest request);
}