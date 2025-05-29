namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class RegistrationMaterialWasteLicence
{
    public Guid RegistrationId { get; set; }

    public required string OrganisationName { get; init; }

    public string? SiteAddress { get; init; }

    public Guid RegistrationMaterialId { get; set; }

    public required string PermitType { get; set; }

    public required string[] LicenceNumbers { get; set; }

    public decimal? CapacityTonne { get; set; }

    public string? CapacityPeriod { get; set; }

    public decimal MaximumReprocessingCapacityTonne { get; set; }

    public required string MaximumReprocessingPeriod { get; set; }

    public required string MaterialName { get; set; }
    public Guid? RegulatorApplicationTaskStatusId { get; init; }

    public RegulatorTaskStatus TaskStatus { get; init; }
}
