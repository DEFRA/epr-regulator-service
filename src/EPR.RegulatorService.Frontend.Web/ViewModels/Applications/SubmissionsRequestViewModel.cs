using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications
{
    [ExcludeFromCodeCoverage]
    public class SubmissionsRequestViewModel
    {
        public string? SearchOrganisationName { get; set; } = string.Empty;
        public string? SearchOrganisationId { get; set; } = string.Empty;
        public bool IsDirectProducerChecked { get; set; }
        public bool IsComplianceSchemeChecked { get; set; }
        public bool IsPendingSubmissionChecked { get; set; }
        public bool IsAcceptedSubmissionChecked { get; set; }
        public bool IsRejectedSubmissionChecked { get; set; }
        public int[] SearchSubmissionYears { get; set; }
        public string[] SearchSubmissionPeriods { get; set; }
        public bool ClearFilters { get; set; }
        public bool IsFilteredSearch { get; set; }
        public int? PageNumber { get; set; } = null;
    }
}
