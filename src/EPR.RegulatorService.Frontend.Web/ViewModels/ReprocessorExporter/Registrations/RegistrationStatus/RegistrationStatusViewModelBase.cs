using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

using System.ComponentModel.DataAnnotations;

public abstract class RegistrationStatusViewModelBase
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required ApplicationOrganisationType? ApplicationType { get; init; }
    public int? Day { get; init; }
    public int? Month { get; init; }

    [Range(2000, 2100, ErrorMessage = "Enter a valid year in YYYY format")]
    public int? Year { get; init; }
}