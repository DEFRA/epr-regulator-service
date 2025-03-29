using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class Registration
{
    public int Id { get; init; }

    public required string OrganisationName { get; init; } = string.Empty;

    public string? SiteAddress { get; init; }

    public ApplicationOrganisationType OrganisationType { get; init; }

    public required string Regulator { get; init; }

    public List<RegistrationMaterial> RegistrationMaterials { get; set; } = [];
}