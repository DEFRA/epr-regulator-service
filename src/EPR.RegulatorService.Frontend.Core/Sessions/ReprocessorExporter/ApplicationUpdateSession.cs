using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class ApplicationUpdateSession
{
    public required int RegistrationMaterialId { get; init; }

    public required int RegistrationId { get; init; }

    public required string MaterialName { get; init; }

    public ApplicationStatus? Status { get; set; }
}