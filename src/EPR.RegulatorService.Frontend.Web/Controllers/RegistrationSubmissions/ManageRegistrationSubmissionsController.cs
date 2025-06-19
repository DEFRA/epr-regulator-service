namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using AutoMapper;

    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;

    public class ManageRegistrationSubmissionsController(
        IManageRegistrationSubmissionsService manageRegistrationSubmissionsService,
        IMapper mapper) : Controller
    {
        private readonly IManageRegistrationSubmissionsService _manageRegistrationSubmissionsService = manageRegistrationSubmissionsService;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.ManageRegistrationSubmissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ManageRegistrationSubmissions(int? pageNumber)
        {
            var registrationSubmissionsFacadeResponse = _manageRegistrationSubmissionsService.GetRegistrationSubmissionsAsync(pageNumber);

            var viewModel = _mapper.Map<RegistrationSubmissionsViewModel>(registrationSubmissionsFacadeResponse);
            return View(viewModel);
        }
    }
}
