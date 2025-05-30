using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterRegistrations}/{PagePath.ManageRegistrations}")]
public class ManageRegistrationsController(IReprocessorExporterService reprocessorExporterService,
    IMapper mapper,
    IValidator<IdRequest> validator,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration) : ReprocessorExporterBaseController(sessionManager, configuration)
{
    private readonly IReprocessorExporterService _reprocessorExporterService = reprocessorExporterService ?? throw new ArgumentNullException(nameof(reprocessorExporterService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<IdRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Guid id)
    {
        await _validator.ValidateAndThrowAsync(new IdRequest { Id = id });

        var registration = await _reprocessorExporterService.GetRegistrationByIdAsync(id);

        ViewBag.BackLinkToDisplay = "";

        var model = _mapper.Map<ManageRegistrationsViewModel>(registration);

        var session = await GetSession();
        session.ReprocessorExporterSession = new ReprocessorExporterSession();

        await SaveSessionAndJourney(session, $"{PagePath.ManageRegistrations}?id={id}");

        return View("~/Views/ReprocessorExporter/Registrations/ManageRegistrations.cshtml", model);
    }
}