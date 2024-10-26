using System.Drawing.Drawing2D;
using System.Diagnostics;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

using ServiceRole = EPR.RegulatorService.Frontend.Core.Enums.ServiceRole;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;

namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;

[FeatureGate(FeatureFlags.ManageRegistrationSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class RegistrationSubmissionsController(
                IFacadeService facade,
                ISessionManager<JourneySession> sessionManager,
                ILogger<RegistrationSubmissionsController> logger,
                IConfiguration configuration,
                IOptions<ExternalUrlsOptions> externalUrlsOptions
             ) : Controller
{
    private readonly string _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    private readonly ExternalUrlsOptions _externalUrlsOptions = externalUrlsOptions.Value;
    private readonly ISessionManager<JourneySession> _sessionManager = sessionManager ?? new JourneySessionManager();
    private readonly IFacadeService _facadeService = facade;
    private JourneySession _currentSession;

    public ISessionManager<JourneySession> SessionManager => _sessionManager;

    private static readonly Action<ILogger, string, Exception?> _logControllerError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(RegistrationSubmissionsController)),
            "An error occurred while processing a message: {ErrorMessage}");

    [HttpGet]
    [Consumes("application/json")]
    [Route(PagePath.RegistrationSubmissionsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationSubmissions(int? pageNumber)
    {
        try
        {
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

            InitialiseOrContinuePaging(_currentSession.RegulatorRegistrationSubmissionSession, pageNumber);

            ViewBag.PowerBiLogin = _externalUrlsOptions.PowerBiLogin;

            SetBacklinkToHome();

            var viewModel = InitialiseOrCreateViewModel(_currentSession.RegulatorRegistrationSubmissionSession);

            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            _logControllerError.Invoke(logger, $"Exception received processing GET to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationSubmissions([FromForm] RegistrationSubmissionsFilterViewModel? filters = null,
                                                             [FromForm] string? filterType = null)
    {
        try
        {
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (ReturnIfAppropriate(filters, filterType) is IActionResult response)
            {
                return response;
            }

            ClearFilters(_currentSession.RegulatorRegistrationSubmissionSession,
                               filters,
                               filterType == FilterActions.ClearFilters);
            UpdateRegistrationSubmissionFiltersInSession(_currentSession.RegulatorRegistrationSubmissionSession,
                               filters,
                               filterType == FilterActions.SubmitFilters);
            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            _logControllerError.Invoke(logger, $"Exception received processing POST to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }

        return RedirectToAction(PagePath.RegistrationSubmissionsAction);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionDetails + "/{organisationId:guid}", Name = "SubmissionDetails")]
    public async Task<IActionResult> RegistrationSubmissionDetails(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetAndRememberOrganisationDetails(organisationId, out var model))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(PagePath.RegistrationSubmissionsRoute);
        ViewBag.OrganisationId = model.OrganisationId;

        return View(nameof(RegistrationSubmissionDetails), model);
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionDetails + "/{organisationId:guid}", Name = "SubmitPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel model, [FromRoute] Guid? organisationid)
    {
        if (!GetAndRememberOrganisationDetails(organisationid, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        existingModel.PaymentDetails = model;

        if (!ModelState.IsValid)
        {
            return View(nameof(RegistrationSubmissionDetails), existingModel);
        }

        // otherwise we will redirect to the confirmation page
        return View(nameof(RegistrationSubmissionDetails), existingModel);
    }

    [HttpGet]
    [Route(PagePath.QueryRegistrationSubmission)]
    public async Task<IActionResult> QueryRegistrationSubmission()
    {
        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{Guid.NewGuid()}");

        var model = new QueryRegistrationSubmissionViewModel();

        return View(nameof(QueryRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationSubmission)]
    public async Task<IActionResult> QueryRegistrationSubmission(QueryRegistrationSubmissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{Guid.NewGuid()}");
            return View(nameof(QueryRegistrationSubmission), model);
        }

        return Redirect(PagePath.RegistrationSubmissionsRoute);
    }

    [HttpGet]
    [Route(PagePath.RejectRegistrationSubmission)]
    public async Task<IActionResult> RejectRegistrationSubmission()
    {
        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{Guid.NewGuid()}");

        var model = new RejectRegistrationSubmissionViewModel();

        return View(nameof(RejectRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectRegistrationSubmission)]
    public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{Guid.NewGuid()}");
            return View(nameof(RejectRegistrationSubmission), model);
        }

        return Redirect(PagePath.RegistrationSubmissionsRoute);
    }

    [HttpGet]
    [Route(PagePath.PageNotFoundPath)]
    public async Task<IActionResult> PageNotFound()
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        _currentSession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 1;

        SetBacklinkToHome();

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.PageNotFound);
        return RedirectToAction(PagePath.Error, "Error", new { statusCode = 404, backLink = PagePath.RegistrationSubmissionsRoute });
    }

    //private RegistrationSubmissionDetailsViewModel GetViewModel(Guid? organisationId) => new RegistrationSubmissionDetailsViewModel
    //{
    //    OrganisationId = organisationId ?? Guid.NewGuid(),
    //    OrganisationReference = "215 148",
    //    OrganisationName = "Acme org Ltd.",
    //    RegistrationReferenceNumber = "REF001",
    //    ApplicationReferenceNumber = "REF002",
    //    OrganisationType = RegistrationSubmissionOrganisationType.large,
    //    BusinessAddress = new BusinessAddress
    //    {
    //        BuildingName = string.Empty,
    //        BuildingNumber = "10",
    //        Street = "High Street",
    //        County = "Randomshire",
    //        PostCode = "A12 3BC"
    //    },
    //    CompaniesHouseNumber = "0123456",
    //    RegisteredNation = "Scotland",
    //    PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
    //    Status = RegistrationSubmissionStatus.queried,
    //    SubmissionDetails = new SubmissionDetailsViewModel
    //    {
    //        Status = RegistrationSubmissionStatus.queried,
    //        DecisionDate = new DateTime(2024, 10, 21, 16, 23, 42, DateTimeKind.Utc),
    //        TimeAndDateOfSubmission = new DateTime(2024, 7, 10, 16, 23, 42, DateTimeKind.Utc),
    //        SubmittedOnTime = true,
    //        SubmittedBy = "Sally Smith",
    //        AccountRole = ServiceRole.ApprovedPerson,
    //        Telephone = "07553 937 831",
    //        Email = "sally.smith@email.com",
    //        DeclaredBy = "Sally Smith",
    //        Files =
    //            [
    //                new() { Label = "SubmissionDetails.OrganisationDetails", FileName = "org.details.acme.csv", DownloadUrl = "#" },
    //                new() { Label = "SubmissionDetails.BrandDetails", FileName = "brand.details.acme.csv", DownloadUrl = "#" },
    //                new() { Label = "SubmissionDetails.PartnerDetails", FileName = "partner.details.acme.csv", DownloadUrl = "#" }
    //            ]
    //    },
    //    PaymentDetails = new PaymentDetailsViewModel
    //    {
    //        ApplicationProcessingFee = 134522.56M,
    //        OnlineMarketplaceFee = 2534534.23M,
    //        SubsidiaryFee = 1.34M,
    //        PreviousPaymentsReceived = 20M
    //    },
    //    ProducerComments = "producer comment",
    //    RegulatorComments = "regulator comment",
    //    BackToAllSubmissionsUrl = GetCustomBackLink(PagePath.RegistrationSubmissionsRoute)
    //};
}
