using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationTask
{
    public int? Id { get; init; }
    
    public RegulatorTaskType TaskName { get; init; }

    public RegulatorTaskStatus Status { get; init; }
}