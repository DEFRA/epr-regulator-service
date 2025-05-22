namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using System;

public class AccreditationTask
{
    public int Id { get; set; } // this will be removed

    public Guid IdGuid { get; set; } // this will be replaced

    public int TaskId { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Year { get; set; }
}