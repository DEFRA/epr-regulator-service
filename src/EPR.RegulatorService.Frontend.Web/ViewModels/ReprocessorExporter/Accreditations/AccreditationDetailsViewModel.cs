using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Helpers;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class AccreditationDetailsViewModel
{
    public Guid Id { get; set; }

    public string ApplicationReference { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; }

    public int AccreditationYear { get; init; }

    public DateTime? DeterminationDate { get; set; }

    public AccreditationTaskViewModel? PRNTonnageTask { get; set; }

    public AccreditationTaskViewModel? BusinessPlanTask { get; set; }

    public AccreditationTaskViewModel? SamplingAndInspectionPlanTask { get; set; }

    public AccreditationTaskViewModel? CheckAccreditationStatusTask { get; set; }

    public AccreditationTaskViewModel? PERNsTonnageAndAuthorityToIssuePERNsTask { get; set; }

    public AccreditationTaskViewModel? OverseasReprocessingSitesAndEvidenceOfBroadlyEquivalentStandardsTask { get; set; }

    public bool ShouldDisplay =>
        AccreditationDisplayHelper.ShouldDisplayAccreditation(Status);
}