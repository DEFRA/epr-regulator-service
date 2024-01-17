using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications
{
    [ExcludeFromCodeCoverage]
    public class ApplicationsRequestViewModel
    {
        public string? SearchOrganisationName { get; set; }
        public bool IsApprovedUserTypeChecked { get; set; }
        public bool IsDelegatedUserTypeChecked { get; set; }
        public bool ClearFilters { get; set; }
        public bool IsFilteredSearch { get; set; }
        public int? PageNumber { get; set; } = null;
    }

}
