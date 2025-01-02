using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Core.Services;

public interface IPaymentFacadeService
{
    Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request);

    Task<T?> GetProducerPaymentDetailsAsync<T>(ProducerPaymentRequest request);

    Task<T?> GetCompliancePaymentDetailsAsync<T>(CompliancePaymentRequest request);
}