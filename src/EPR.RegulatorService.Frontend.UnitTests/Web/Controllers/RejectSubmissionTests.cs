using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.Constants;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class RejectSubmissionTests : SubmissionsTestBase
    {
        private const string ViewName = "RejectSubmission";
        private readonly Submission _testSubmission = TestSubmission.GetTestSubmission();

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            JourneySessionMock = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    Journey =
                        new List<string>
                        {
                            PagePath.Submissions, PagePath.SubmissionDetails, PagePath.RejectSubmission
                        },
                    RejectSubmissionJourneyData = new RejectSubmissionJourneyData()
                    {
                        OrganisationName = _testSubmission.OrganisationName,
                        SubmissionId = _testSubmission.SubmissionId,
                        SubmittedBy = _testSubmission.FirstName + " " + _testSubmission.LastName,
                    },
                    OrganisationSubmission = new Submission()
                    {
                        FileId = Guid.NewGuid(),
                        OrganisationId = Guid.NewGuid(),
                        OrganisationName = _testSubmission.OrganisationName,
                        SubmissionId = _testSubmission.SubmissionId
                    }
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnRejectSubmissionsPage_WhenRejectSubmissionsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Act
            var result = await _systemUnderTest.RejectSubmission() as ViewResult;
            var rejectSubmissionJourneyData = JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            var model = result.Model as RejectSubmissionViewModel;
            Assert.IsNotNull(model);
            model.OrganisationName.Should().Be(rejectSubmissionJourneyData!.OrganisationName);
            model.SubmissionId.Should().Be(rejectSubmissionJourneyData.SubmissionId);
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
                OrganisationName = rejectSubmissionJourneyData!.OrganisationName!,
                SubmissionId = rejectSubmissionJourneyData.SubmissionId,
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
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: RejectSubmissionDetails.ModelErrorValueNoRejectionReason
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
                OrganisationName = rejectSubmissionJourneyData!.OrganisationName!,
                SubmissionId = rejectSubmissionJourneyData.SubmissionId,
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
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: RejectSubmissionDetails.ModelErrorValueRejectionReasonTooLong
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
                OrganisationName = rejectSubmissionJourneyData!.OrganisationName!,
                SubmissionId = rejectSubmissionJourneyData.SubmissionId,
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
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: RejectSubmissionDetails.ModelErrorValueNoResubmissionOptionSelected
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
                OrganisationName = rejectSubmissionJourneyData!.OrganisationName!,
                SubmissionId = rejectSubmissionJourneyData.SubmissionId,
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy!,
                ReasonForRejection = RejectSubmissionDetails.RejectionReasonString,
                IsResubmissionRequired = false
            };

            _facadeServiceMock.Setup(x => x.SubmitPoMDecision(It.IsAny<RegulatorPoMDecisionCreateRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _systemUnderTest.RejectSubmission(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            result.ActionName.Should().Be(nameof(SubmissionsController.Submissions));

            var routeValues = result.RouteValues;
            Assert.IsNotNull(routeValues);
            routeValues.Values.Should().Contain(EndpointResponseStatus.Success);
            routeValues.Values.Should().Contain(rejectSubmissionJourneyData.OrganisationName);

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }

        [TestMethod]
        public async Task PreSubmissionDecision_WhenCalled_CreatesRejectSubmissionJourneyDataAndRedirectsToRejectSubmission()
        {
            var result = await _systemUnderTest.PreSubmissionDecision("OrganisationName", Guid.NewGuid(), "Submitter");

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            Assert.IsNotNull(actionResult);
            actionResult.ActionName.Should().Be("RejectSubmission");
        }
    }
}