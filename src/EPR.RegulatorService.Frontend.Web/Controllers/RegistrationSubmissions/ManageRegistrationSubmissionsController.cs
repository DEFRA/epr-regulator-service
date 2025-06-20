namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Mappings.ManageRegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;

    public class ManageRegistrationSubmissionsController(
        IManageRegistrationSubmissionsService manageRegistrationSubmissionsService) : Controller
    {
        private readonly IManageRegistrationSubmissionsService _manageRegistrationSubmissionsService = manageRegistrationSubmissionsService;

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.ManageRegistrationSubmissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ManageRegistrationSubmissions(int? pageNumber)
        {
            var registrationSubmissionsFacadeResponse = await _manageRegistrationSubmissionsService.GetRegistrationSubmissionsAsync(pageNumber);

            var viewModel = RegistrationSubmissionsViewModelMapper.MapToViewModel(registrationSubmissionsFacadeResponse);

            return View(viewModel);
        }

        [HttpGet]
        [Route(PagePath.RegistrationSubmissionDetailsNew + "/{submissionId:guid}")]
        public async Task<IActionResult> RegistrationSubmissionDetailsNew(Guid submissionId)
        {
            var registrationSubmissionDetailsFacadeResponse = await _manageRegistrationSubmissionsService.GetRegistrationSubmissionDetailsAsync(submissionId);

            var viewModel = RegistrationSubmissionDetailsModelMapper.MapToViewModel(registrationSubmissionDetailsFacadeResponse);

            return View(viewModel);
        }
    }
}
