using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialOutcomeRequest
{
    public ApplicationStatus? Status { get; init; }

    public string? Comments { get; init; }
}