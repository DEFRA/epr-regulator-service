namespace EPR.RegulatorService.Frontend.Web.Controllers.RegulatorEnrolment
{
    using System.Security.Claims;

    using Constants;

    using Core.Models;
    using Core.Services;
    using Core.Sessions;

    using Extensions;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;

    using ViewModels.RegulatorEnrolment;

    [Route("")]
    public class RegulatorEnrolmentController : Controller
    {
        private readonly ILogger<RegulatorEnrolmentController> _logger;
        private readonly IFacadeService _facadeService;

        public RegulatorEnrolmentController(ILogger<RegulatorEnrolmentController> logger,
            IFacadeService facadeService)
        {
            _logger = logger;
            _facadeService = facadeService;
        }

        [HttpGet]
        [Route(PagePath.FullName)]
        public async Task<ActionResult> FullName(string? token)
        {
            var session = new JourneySession
            {
                UserData = HttpContext.User.GetInvitedUserData()
            };

            if (string.IsNullOrEmpty(token))
            {
                _logger.InvitedTokenWasNotProvided(session.UserData.Id);

                return RedirectToAction(PagePath.Error, "Error");
            }

            session.UserData.InviteToken = token;

            await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(HttpContext, session.UserData);

            ViewBag.CurrentPage = $"{HttpContext.Request.PathBase}{HttpContext.Request.Path}?{nameof(token)}={token}";

            return View(new FullNameViewModel { Token = token });
        }

        [HttpPost]
        [Route(PagePath.FullName)]
        public async Task<ActionResult> FullName(FullNameViewModel model)
        {
            ViewBag.CurrentPage = $"{HttpContext.Request.PathBase}{HttpContext.Request.Path}?token={HttpContext.User.GetUserData().InviteToken}";

            if (ModelState.IsValid)
            {
                var session = new JourneySession
                {
                    UserData = HttpContext.User.GetUserData()
                };

                var request = new EnrolInvitedUserRequest
                {
                    InviteToken = session.UserData.InviteToken!,
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value,
                    UserId = Guid.Parse(HttpContext.User.Claims.Single(claim => claim.Type == ClaimConstants.ObjectId).Value)
                };

                var result = await _facadeService.EnrolInvitedUser(request);

                if (result == EndpointResponseStatus.Success)
                {
                    _logger.EnrolmentForInvitedUserSuccessful(request.UserId);

                    session.UserData.FirstName = request.FirstName;
                    session.UserData.LastName = request.LastName;

                    await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(HttpContext, session.UserData);

                    _logger.UserDataUpdated(request.UserId);

                    return RedirectToAction(PagePath.LandingPage, "Home");
                }

                _logger.EnrolmentForInvitedUserFailed(request.UserId);

                ModelState.AddModelError("Error", "Unable to enrol invited user");
            }

            return View(model);
        }
    }
}