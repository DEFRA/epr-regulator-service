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
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // If ID is invalid
    [ProducesResponseType(StatusCodes.Status404NotFound)] // If registration is not found
    public IActionResult Index([FromQuery] int id)
    {
        try
        {
            // Validate ID
            if (id <= 0)
            {
                _logger.LogWarning("Invalid ID received in query string: {Id}", id);
                return BadRequest("Invalid registration ID.");
            }

            var registration = _registrationService.GetRegistrationById(id);

            if (registration == null)
            {
                _logger.LogWarning("No registration found for ID: {Id}", id);
                return NotFound("Registration not found.");
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
            // Log and debug exception
            Debug.WriteLine(ex);
            _logger.LogError(ex, "Exception occurred while processing ManageRegistrations request for ID: {Id}", id);

            // Redirect to the error page to maintain a consistent user experience
            return RedirectToAction(PagePath.Error, "Error");
        }
    }
}