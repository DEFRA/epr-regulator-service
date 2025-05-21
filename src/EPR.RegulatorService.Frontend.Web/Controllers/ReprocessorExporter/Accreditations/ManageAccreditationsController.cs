using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterAccreditations}/{PagePath.ManageAccreditations}")]
public class ManageAccreditationsController(
    IReprocessorExporterService reprocessorExporterService,
    IMapper mapper,
    IValidator<IdRequest> validator,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration) : ReprocessorExporterBaseController(sessionManager, configuration)
{
    private readonly IReprocessorExporterService _reprocessorExporterService = reprocessorExporterService ?? throw new ArgumentNullException(nameof(reprocessorExporterService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<IdRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int id)
    {
        await _validator.ValidateAndThrowAsync(new IdRequest { Id = id });

        // You may want to use a different service method if accreditations are separate from registrations
        var registration = await _reprocessorExporterService.GetRegistrationByDateAsync(id);

        ViewBag.BackLinkToDisplay = "";

        var model = _mapper.Map<ManageAccreditationsViewModel>(registration);

        var session = await GetSession();
        session.ReprocessorExporterSession = new ReprocessorExporterSession();

        await SaveSessionAndJourney(session, $"{PagePath.ManageAccreditations}?id={id}");

        return View("~/Views/ReprocessorExporter/Accreditations/ManageAccreditations.cshtml", model);
    }
}