namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class AccreditationDetailsViewModel
{
    public Guid Id { get; set; }

    public string ApplicationReference { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int AccreditationYear { get; init; }

    public DateTime? DeterminationDate { get; set; }

    public AccreditationTaskViewModel? PRNTonnageTask { get; set; }

    public AccreditationTaskViewModel? BusinessPlanTask { get; set; }

    public AccreditationTaskViewModel? SamplingAndInspectionPlanTask { get; set; }

}