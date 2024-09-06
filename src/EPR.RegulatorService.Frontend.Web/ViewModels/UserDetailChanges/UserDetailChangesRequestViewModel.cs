using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges
{
    [ExcludeFromCodeCoverage]
    public class UserDetailChangesRequestViewModel
    {
        public ChangeHistoryModel? ChangeHistoryModel { get; set; }
    }

}
