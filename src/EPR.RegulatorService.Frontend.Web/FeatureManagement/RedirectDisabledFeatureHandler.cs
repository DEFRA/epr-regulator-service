namespace EPR.RegulatorService.Frontend.Web.FeatureManagement
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.FeatureManagement.Mvc;
    using System.Collections.Generic;
    using EPR.RegulatorService.Frontend.Web.Constants;

    public class RedirectDisabledFeatureHandler : IDisabledFeaturesHandler
    {
        public Task HandleDisabledFeatures(IEnumerable<string> features, ActionExecutingContext context)
        {
            context.Result = new RedirectToActionResult("LandingPage", PagePath.Home,null);
            return Task.CompletedTask;
        }
    }
}
