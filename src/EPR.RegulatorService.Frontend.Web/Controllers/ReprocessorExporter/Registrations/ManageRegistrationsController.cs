using System.Diagnostics;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterRegistrations}/{PagePath.ManageRegistrations}")]
public class ManageRegistrationsController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<ManageRegistrationsController> _logger;

    public ManageRegistrationsController(IRegistrationService registrationService, ILogger<ManageRegistrationsController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]  // When the view loads successfully
    [ProducesResponseType(StatusCodes.Status302Found)] // When redirecting to the error page
    public IActionResult Index([FromQuery] int id)
    {
        try
        {
            // Validate ID
            if (id <= 0)
            {
                _logger.LogError("Invalid ID received in query string: {Id}", id);
                return RedirectToAction(PagePath.Error, "Error");
            }

            var registration = _registrationService.GetRegistrationById(id);

            if (registration == null)
            {
                _logger.LogError("No registration found for ID: {Id}", id);
                return RedirectToAction(PagePath.Error, "Error");
            }

            ViewBag.BackLinkToDisplay = "";

            var model = new ManageRegistrationsViewModel
            {
                Id = registration.Id,
                OrganisationName = registration.OrganisationName,
                SiteAddress = registration.SiteAddress,
                ApplicationOrganisationType = registration.OrganisationType,
                Regulator = registration.Regulator
            };

            return View("~/Views/ReprocessorExporter/Registrations/ManageRegistrations.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing ManageRegistrations request for ID: {Id}", id);
            return RedirectToAction(PagePath.Error, "Error");
        }
    }
}