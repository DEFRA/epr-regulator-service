namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using System.Net;

    using EPR.RegulatorService.Frontend.Core.Exceptions.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Logging.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Mappings.ManageRegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;

    public class ManageRegistrationSubmissionsController(
        IManageRegistrationSubmissionsService manageRegistrationSubmissionsService,
        ILogger<ManageRegistrationSubmissionsController> logger) : Controller
    {
        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.ManageRegistrationSubmissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ManageRegistrationSubmissions(int? pageNumber)
        {
            try
            {
                var registrationSubmissionsFacadeResponse = await manageRegistrationSubmissionsService.GetRegistrationSubmissionsAsync(pageNumber);

                var viewModel = RegistrationSubmissionsViewModelMapper.MapToViewModel(registrationSubmissionsFacadeResponse);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.ErrorFetchingSubmissions(pageNumber, ex);
                return RedirectToAction(PagePath.Error, "Error");
            }
        }

        [HttpGet]
        [Route(PagePath.RegistrationSubmissionDetailsNew + "/{submissionId:guid}")]
        public async Task<IActionResult> RegistrationSubmissionDetailsNew(Guid submissionId)
        {
            try
            {
                var registrationSubmissionDetailsFacadeResponse = await manageRegistrationSubmissionsService.GetRegistrationSubmissionDetailsAsync(submissionId);
                var viewModel = RegistrationSubmissionDetailsModelMapper.MapToViewModel(registrationSubmissionDetailsFacadeResponse);
                return View(viewModel);
            }
            catch (RegistrationSubmissionNotFoundException)
            {
                return RedirectToAction(PagePath.Error, "Error",
                    new
                    {
                        statusCode = 404,
                        backLink = PagePath.ManageRegistrationSubmissions
                    });
            }
            catch (HttpRequestException ex)
            {
                logger.HttpErrorFetchingSubmissionDetails(submissionId, ex);
                return RedirectToAction(PagePath.Error, "Error");
            }
            catch (Exception ex)
            {
                logger.UnexpectedErrorFetchingSubmissionDetails(submissionId, ex);
                return RedirectToAction(PagePath.Error, "Error");
            }
        }
    }
}
