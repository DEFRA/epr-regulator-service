using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class WasteCarrierDetails : RegistrationSectionBase
{
    public string? WasteCarrierBrokerDealerNumber { get; init; }
    public Guid? RegulatorRegistrationTaskStatusId { get; init; }
}