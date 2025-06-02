using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class AuthorisedMaterialsViewModel
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public required List<AuthorisedMaterialViewModel> Materials { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}