using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class WasteCarrierDetailsViewModel
{
    public required Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public string? WasteCarrierBrokerDealerNumber { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}