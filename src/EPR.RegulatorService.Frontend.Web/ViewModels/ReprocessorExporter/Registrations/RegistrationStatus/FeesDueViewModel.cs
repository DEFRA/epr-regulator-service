namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

public class FeesDueViewModel : RegistrationStatusViewModelBase
{
    public string ApplicationReferenceNumber { get; init; }
    public int RegistrationMaterialId { get; init; }
    public string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
}