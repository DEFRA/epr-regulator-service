using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.UnitTests.Constants;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class RejectSubmissionTests : SubmissionsTestBase
    {
        private const string ViewName = "RejectSubmission";
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
                    Journey =
                        [
                        PagePath.Submissions,
                        PagePath.SubmissionDetails,
                        PagePath.RejectSubmission
                        ],
                    RejectSubmissionJourneyData = new RejectSubmissionJourneyData()
                    {
                        SubmittedBy = testSubmission.FirstName + " " + testSubmission.LastName,
                    }
                }
            };

            testSubmission.FileId = Guid.NewGuid();
            testSubmission.OrganisationId = Guid.NewGuid();

            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode] = testSubmission;

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Act
            var result = await _systemUnderTest.RejectSubmission(_hashCode) as ViewResult;
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            var model = result.Model as RejectSubmissionViewModel;
            Assert.IsNotNull(model);
            model.SubmittedBy.Should().Be(rejectSubmissionJourneyData.SubmittedBy);
            AssertBackLink(result, PagePath.SubmissionDetails);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpPostCalled_NoRejectionReasonEntered_ThenThrowValidationError()
        {
            // Arrange
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;

            // Act
            var viewModel = new RejectSubmissionViewModel()
            {
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy!,
                ReasonForRejection = null,
                IsResubmissionRequired = false
            };

            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, RejectSubmissionDetails.ModelErrorValueNoRejectionReason);

            var result = await _systemUnderTest.RejectSubmission(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            Assert.AreEqual(
                expected: RejectSubmissionDetails.ModelErrorValueNoRejectionReason,
                actual: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpPostCalled_RejectionReasonEnteredTooLong_ThenThrowValidationError()
        {
            // Arrange
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;

            // Act
            var viewModel = new RejectSubmissionViewModel()
            {
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy!,
                ReasonForRejection = RejectSubmissionDetails.LongRejectionReasonString,
                IsResubmissionRequired = false
            };

            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, RejectSubmissionDetails.ModelErrorValueRejectionReasonTooLong);
            var result = await _systemUnderTest.RejectSubmission(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            Assert.AreEqual(
                expected: RejectSubmissionDetails.ModelErrorValueRejectionReasonTooLong,
                actual: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpPostCalled_NoResubmissionOptionSelected_ThenThrowValidationError()
        {
            // Arrange
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;

            // Act
            var viewModel = new RejectSubmissionViewModel()
            {
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy!,
                ReasonForRejection = RejectSubmissionDetails.RejectionReasonString,
                IsResubmissionRequired = false
            };

            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, RejectSubmissionDetails.ModelErrorValueNoResubmissionOptionSelected);
            var result = await _systemUnderTest.RejectSubmission(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            Assert.AreEqual(
                expected: RejectSubmissionDetails.ModelErrorValueNoResubmissionOptionSelected,
                actual: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpPostCalled_HappyPath_ThenRedirectToSubmissionsPage_AndUpdateSession()
        {
            // Arrange
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;
            var viewModel = new RejectSubmissionViewModel()
            {
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy!,
                ReasonForRejection = RejectSubmissionDetails.RejectionReasonString,
                IsResubmissionRequired = false
            };

            _facadeServiceMock.Setup(x => x.SubmitPoMDecision(It.IsAny<RegulatorPoMDecisionCreateRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _systemUnderTest.RejectSubmission(viewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBe(null);
            result.ActionName.Should().Be(nameof(SubmissionsController.Submissions));

            _systemUnderTest.TempData["SubmissionResultReject"].Should().Be(EndpointResponseStatus.Success);
            _systemUnderTest.TempData["SubmissionResultOrganisationName"].Should().Be("Test Org Ltd.");

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }
    }
}