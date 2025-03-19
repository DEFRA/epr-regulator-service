using System;
using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;

public class Registration
{
    public int Id { get; init; }

    [Required]
    public string OrganisationName { get; init; } = string.Empty;

    public string? SiteAddress { get; init; }

    public ApplicationOrganisationType OrganisationType { get; init; }

    public string Regulator { get; init; } = "Environment Agency (EA)";
}

