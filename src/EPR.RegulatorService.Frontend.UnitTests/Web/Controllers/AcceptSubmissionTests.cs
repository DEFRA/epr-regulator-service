using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.UnitTests.Constants;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using Frontend.Core.Models.Submissions;

    [TestClass]
    public class AcceptSubmissionTests : SubmissionsTestBase
    {
        private const string ViewName = "AcceptSubmission";

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            JourneySessionMock = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    Journey = new List<string>
                    {
                        PagePath.Submissions,
                        PagePath.SubmissionDetails,
                        PagePath.AcceptSubmission
                    },
                    OrganisationSubmission = TestSubmission.GetTestSubmission()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;

            // Act
            var result = await _systemUnderTest.AcceptSubmission() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().NotBeNull();
            result.ViewName.Should().Be(ViewName);

            var model = result.Model as AcceptSubmissionViewModel;
            model!.OrganisationName.Should().Be(organisationSubmission.OrganisationName);
            AssertBackLink(result, PagePath.SubmissionDetails);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpPostCalled_NoVerificationOptionSelected_ThenThrowValidationError()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;
            var viewModel = new AcceptSubmissionViewModel()
            {
                OrganisationName = organisationSubmission.OrganisationName,
            };

            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, AcceptSubmissionDetails.ModelErrorValueAcceptVerificationNotSelected);

            // Act
            var result = await _systemUnderTest.AcceptSubmission(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewName);
            result.ViewName.Should().Be(ViewName);
            Assert.AreEqual(
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: AcceptSubmissionDetails.ModelErrorValueAcceptVerificationNotSelected
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpPostCalled_HappyPath_NoSelected_ThenRedirectToSubmissionDetailsPage()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;

            var viewModel = new AcceptSubmissionViewModel()
            {
                OrganisationName = organisationSubmission.OrganisationName,
                Accepted = false
            };

            // Act
            var result = await _systemUnderTest.AcceptSubmission(viewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().NotBeNull();
            result.ActionName.Should().Be(nameof(SubmissionsController.SubmissionDetails));

            var routeValues = result.RouteValues;
            routeValues.Should().BeNull();

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpPostCalled_HappyPath_YesSelected_ThenRedirectToSubmissionsPage_AndUpdateSession()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;
            var viewModel = new AcceptSubmissionViewModel()
            {
                OrganisationName = organisationSubmission.OrganisationName,
                Accepted = true
            };

            _facadeServiceMock.Setup(x => x.SubmitPoMDecision(It.IsAny<RegulatorPoMDecisionCreateRequest>())).ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _systemUnderTest.AcceptSubmission(viewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().NotBeNull();
            result.ActionName.Should().Be(nameof(SubmissionsController.Submissions));

            var routeValues = result.RouteValues;
            routeValues.Should().NotBeNull();
            routeValues!.Values.Should().Contain(EndpointResponseStatus.Success);
            routeValues.Values.Should().Contain(organisationSubmission.OrganisationName);

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }
    }
}