using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents
{
    using Core.Models;

    public class EnrolmentRequestDetailsViewComponent : ViewComponent
    {
        public async Task<ViewViewComponentResult> InvokeAsync(bool isApprovedPersonAccepted, User  approvedUser, List<User> delegatedUsers)
        {
            var model = new EnrolmentRequestDetailsModel
            {
                IsApprovedUserAccepted = isApprovedPersonAccepted,
                ApprovedUser = approvedUser,
                DelegatedUsers = delegatedUsers.ToList()
            };
            
            return View(model);
        }
    }
}

