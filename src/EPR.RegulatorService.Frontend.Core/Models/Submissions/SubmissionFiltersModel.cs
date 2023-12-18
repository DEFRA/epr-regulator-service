namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class SubmissionFiltersModel
{
    public string? SearchOrganisationName { get; set; } = string.Empty;
    public string? SearchOrganisationId { get; set; } = string.Empty;
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingSubmissionChecked { get; set; }
    public bool IsAcceptedSubmissionChecked { get; set; }
    public bool IsRejectedSubmissionChecked { get; set; }
    public bool ClearFilters { get; set; }
    public bool IsFilteredSearch { get; set; }
}