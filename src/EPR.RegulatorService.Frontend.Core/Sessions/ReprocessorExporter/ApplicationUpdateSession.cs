using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class ApplicationUpdateSession
{
    public required Guid RegistrationMaterialId { get; init; }

    public required Guid RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public ApplicationStatus? Status { get; set; }
}