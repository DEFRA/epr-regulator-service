using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterial
{
    public int Id { get; init; }

    public int RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public DateTime? DeterminationDate { get; set; }
    
    public ApplicationStatus? Status { get; set; }

    public string? StatusUpdatedByName { get; init; }

    public DateTime? StatusUpdatedAt { get; init; }

    public string? RegistrationNumber { get; init; }

    public List<RegistrationTask> Tasks { get; set; } = [];
}