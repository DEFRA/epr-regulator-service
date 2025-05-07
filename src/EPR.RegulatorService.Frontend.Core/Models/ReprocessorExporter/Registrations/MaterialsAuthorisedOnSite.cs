namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class MaterialsAuthorisedOnSite
{
    public required string MaterialName { get; set; }
    public string? Reason { get; set; }
    public bool IsMaterialRegistered { get; set; }
}
