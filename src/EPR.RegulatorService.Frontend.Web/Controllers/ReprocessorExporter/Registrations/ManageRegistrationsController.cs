using System.Net;

using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Errors;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterRegistrations}/{PagePath.ManageRegistrations}")]
public class ManageRegistrationsController(IRegistrationService registrationService,
    IMapper mapper,
    IValidator<ManageRegistrationsRequest> validator,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration) : RegulatorSessionBaseController(sessionManager, configuration)

{
    private readonly IRegistrationService _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<ManageRegistrationsRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Index([FromQuery] int id)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session?.ReprocessorExporterSession?.Journey == null)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.InternalServerError });
        }

        _validator.ValidateAndThrow(new ManageRegistrationsRequest { Id = id });

        var registration = _registrationService.GetRegistrationById(id);

        ViewBag.BackLinkToDisplay = "";

        var model = _mapper.Map<ManageRegistrationsViewModel>(registration);

        string manageRegistrationPath = $"{PagePath.ManageRegistrations}?id={id}";
        SaveSessionAndJourney(session, manageRegistrationPath);

        return View("~/Views/ReprocessorExporter/Registrations/ManageRegistrations.cshtml", model);
    }
}