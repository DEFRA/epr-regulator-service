namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using System;

public class AccreditationTask
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Year { get; set; }
}