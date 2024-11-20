using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Services;

public interface IPaymentFacadeService
{
    Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request);

    Task<Optional<ProducerPaymentResponse>> GetProducerPaymentDetailsAsync(ProducerPaymentRequest request);

    Task<Optional<CompliancePaymentResponse>> GetCompliancePaymentDetailsAsync(CompliancePaymentRequest request);
}