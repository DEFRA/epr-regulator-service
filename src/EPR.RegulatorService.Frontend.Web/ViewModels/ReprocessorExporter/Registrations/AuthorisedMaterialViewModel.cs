namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class AuthorisedMaterialViewModel
{
    public required string MaterialName { get; init; }
    public bool IsMaterialRegistered { get; init; }
    public string? Reason { get; init; }
}
