using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterial
{
    public int Id { get; init; }

    public int RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public ApplicationStatus? Status { get; set; }

    public string? Comments { get; set; }

    public List<RegistrationTask> Tasks { get; set; } = [];
}