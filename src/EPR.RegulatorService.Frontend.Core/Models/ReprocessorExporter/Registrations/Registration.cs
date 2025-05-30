using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class Registration
{
    public Guid Id { get; init; }

    public required string OrganisationName { get; init; } = string.Empty;

    public string? SiteAddress { get; init; }

    public string? SiteGridReference { get; set; } = string.Empty;

    public ApplicationOrganisationType OrganisationType { get; init; }

    public required string Regulator { get; init; }

    public List<RegistrationTask> Tasks { get; set; } = [];

    public List<RegistrationMaterialSummary> Materials { get; set; } = [];
}