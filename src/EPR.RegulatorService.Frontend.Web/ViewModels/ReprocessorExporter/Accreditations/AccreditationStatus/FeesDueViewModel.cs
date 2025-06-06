namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class FeesDueViewModel : AccreditationStatusViewModelBase
{
    public string ApplicationReferenceNumber { get; init; }
    public string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
    public string PrnTonnage { get; set; }
}