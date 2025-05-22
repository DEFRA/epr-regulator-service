using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialSummary
{
    public int Id { get; init; } // this will be removed

    public Guid IdGuid { get; init; } // this will be replaced

    public int RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public DateTime? DeterminationDate { get; set; }

    public ApplicationStatus? Status { get; init; }

    public string? StatusUpdatedBy { get; init; }

    public DateTime? StatusUpdatedDate { get; init; }

    public string? RegistrationReferenceNumber { get; init; }

    public string? ApplicationReferenceNumber { get; init; }

    public List<RegistrationTask> Tasks { get; init; } = [];

    // Supports multiple accreditations, one per year
    public List<Accreditation> Accreditations { get; set; } = [];
}