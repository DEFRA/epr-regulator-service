using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges
{
    [ExcludeFromCodeCoverage]
    public class UserDetailChangesListViewModel
    {
        public IEnumerable<OrganisationUserDetailChangeRequest>? PagedPendingUserRequests { get; set; }
        
        public PaginationNavigationModel PaginationNavigationModel { get; set; }
        
        public RegulatorUserDetailChangeRequestFiltersModel UserDetailChangeRequestFiltersModel { get; set; }
    }
}