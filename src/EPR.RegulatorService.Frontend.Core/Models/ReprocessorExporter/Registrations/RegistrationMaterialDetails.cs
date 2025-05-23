using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialDetail
{
    public int Id { get; init; }

    public int RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public ApplicationStatus? Status { get; init; }
}