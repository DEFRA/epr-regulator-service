using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationTask
{
    public int? Id { get; init; } // this will be removed

    public Guid? IdGuid { get; init; } // this will be replaced

    public RegulatorTaskType TaskName { get; init; }

    public RegulatorTaskStatus Status { get; init; }

    public string? Year { get; set; }
}