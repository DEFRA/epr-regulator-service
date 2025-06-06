namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

using Core.Enums.ReprocessorExporter;

public class FeesDueViewModel : RegistrationStatusViewModelBase
{
    public Guid RegistrationId { get; init; }
    public string ApplicationReferenceNumber { get; init; }
    public Guid RegistrationMaterialId { get; init; }
    public string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}