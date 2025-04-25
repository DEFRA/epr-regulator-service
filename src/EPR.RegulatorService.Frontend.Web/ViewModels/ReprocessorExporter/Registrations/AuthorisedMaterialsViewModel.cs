namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class AuthorisedMaterialsViewModel
{
    public int RegistrationId { get; init; }

    public required string OrganisationName { get; init; }

    public required string SiteAddress { get; init; }

    public required List<AuthorisedMaterialViewModel> Materials { get; init; }
}
