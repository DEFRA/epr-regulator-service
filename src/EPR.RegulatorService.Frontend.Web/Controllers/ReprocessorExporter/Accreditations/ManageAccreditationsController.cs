using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
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
    IValidator<IdAndYearRequest> validator,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration) : ReprocessorExporterBaseController(sessionManager, configuration)
{
    private readonly IReprocessorExporterService _reprocessorExporterService = reprocessorExporterService ?? throw new ArgumentNullException(nameof(reprocessorExporterService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<IdAndYearRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Guid id, [FromQuery] int year)
    {
        var request = new IdAndYearRequest { Id = id, Year = year };
        await _validator.ValidateAndThrowAsync(request);

        var registration = await _reprocessorExporterService.GetRegistrationByIdWithAccreditationsAsync(id, year);

        ViewBag.BackLinkToDisplay = "";

        var model = _mapper.Map<ManageAccreditationsViewModel>(registration);

        var session = await GetSession();
        session.ReprocessorExporterSession = new ReprocessorExporterSession();

        await SaveSessionAndJourney(session, $"{PagePath.ManageAccreditations}?id={id}&year={year}");

        return View("~/Views/ReprocessorExporter/Accreditations/ManageAccreditations.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.QueryAccreditationTask)]
    public async Task<IActionResult> QueryAccreditationTask(Guid registrationId, RegulatorTaskType taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryAccreditationTask);
        SetBackLinkInfos(session, PagePath.QueryAccreditationTask);

        var queryAccreditationTaskViewModel = new QueryAccreditationTaskViewModel
        {
            RegistrationId = registrationId,
            TaskName = taskName
        };

        return View("~/Views/ReprocessorExporter/Accreditations/QueryAccreditationTask.cshtml", queryAccreditationTaskViewModel);
    }

    [HttpPost]
    [Route(PagePath.QueryAccreditationTask)]
    public async Task<IActionResult> QueryRegistrationTask(QueryAccreditationTaskViewModel queryAccreditationTaskViewModel)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteQueryAccreditationTask);
        SetBackLinkInfos(session, PagePath.CompleteQueryAccreditationTask);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = queryAccreditationTaskViewModel.TaskName.ToString(),
            RegistrationId = queryAccreditationTaskViewModel.RegistrationId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = queryAccreditationTaskViewModel.Comments
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = queryAccreditationTaskViewModel.RegistrationId });
    }
}
