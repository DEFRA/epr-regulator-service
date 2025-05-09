using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.Constants;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class AcceptSubmissionTests : SubmissionsTestBase
    {
        private const string ViewName = "AcceptSubmission";
        private int _hashCode;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            var testSubmission = TestSubmission.GetTestSubmission();
            _hashCode = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission);

            JourneySessionMock = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    Journey = new List<string>
                    {
                        PagePath.Submissions,
                        PagePath.SubmissionDetails,
                        PagePath.AcceptSubmission
                    }
                }
            };

            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode] = testSubmission;
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode];

            // Act
            var result = await _systemUnderTest.AcceptSubmission(_hashCode) as ViewResult;

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
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode] ;
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
             expected: AcceptSubmissionDetails.ModelErrorValueAcceptVerificationNotSelected,
             actual: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpPostCalled_HappyPath_NoSelected_ThenRedirectToSubmissionDetailsPage()
        {
            // Arrange
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode];

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
            var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode] ;
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
            result.ActionName.Should().Be(nameof(SubmissionsController.Submissions));

            _systemUnderTest.TempData["SubmissionResultAccept"].Should().Be(EndpointResponseStatus.Success);
            _systemUnderTest.TempData["SubmissionResultOrganisationName"].Should().Be("Test Org Ltd.");

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }
    }
}