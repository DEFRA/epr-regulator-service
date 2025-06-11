using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ManageRegistrationsViewModel
{
    public Guid Id { get; set; }

    public required string OrganisationName { get; init; }

    public string? SiteAddress { get; init; } // Only for reprocessors

    public ApplicationOrganisationType ApplicationOrganisationType { get; init; }

    public required string Regulator { get; init; }

    public List<ManageRegistrationMaterialViewModel> Materials { get; init; } = [];

    public RegistrationTaskViewModel? SiteAddressTask { get; set; }

    public RegistrationTaskViewModel? MaterialsAuthorisedOnSiteTask { get; set; }

    public RegistrationTaskViewModel? BusinessAddressTask { get; set; }

    public RegistrationTaskViewModel? ExporterWasteLicensesTask { get; set; }
}