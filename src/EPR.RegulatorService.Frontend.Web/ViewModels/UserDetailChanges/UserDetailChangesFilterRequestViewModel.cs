using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges
{
    [ExcludeFromCodeCoverage]
    public class UserDetailChangesFilterRequestViewModel
    {
        public string? SearchOrganisationName { get; set; }
        public bool IsApprovedUserTypeChecked { get; set; }
        public bool IsDelegatedUserTypeChecked { get; set; }
        public bool ClearFilters { get; set; }
        public bool IsFilteredSearch { get; set; }
        public int? PageNumber { get; set; } = null;
    }

}
