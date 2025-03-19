using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class SubmissionDetailsTests : SubmissionsTestBase
    {
        private const string ViewName = "SubmissionDetails";

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
                        PagePath.SubmissionDetails
                    },
                    OrganisationSubmission = TestSubmission.GetTestSubmission()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnSubmissionDetailsPage_WhenSubmissionDetailsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Arrange
            var submissionFromSession = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;
            _facadeServiceMock.Setup(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null))
                .ReturnsAsync(new PomPayCalParametersResponse {
                    Reference = "12345",
                    MemberCount = 1,
                    ResubmissionDate = DateTime.UtcNow,
                    NationCode = "GB-ENG"
                });

            // Act
            var result = await _systemUnderTest.SubmissionDetails() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewName.Should().NotBeNull();
            result.ViewName.Should().Be(ViewName);

            var model = result!.Model as SubmissionDetailsViewModel;
            model!.OrganisationName.Should().Be(submissionFromSession.OrganisationName);
            model.SubmissionId.Should().Be(submissionFromSession.SubmissionId);
            model.SubmittedBy.Should().Be($"{submissionFromSession.FirstName} {submissionFromSession.LastName}");
            model.ReferenceFieldNotAvailable.Should().BeFalse();
            model.ReferenceNotAvailable.Should().BeFalse();
            AssertBackLink(result, PagePath.Submissions);
            _facadeServiceMock.Verify(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null), Times.Never);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task GivenOnSubmissionDetailsPage_WhenSubmissionDetailsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session_Resubmission(bool withResubmissionDate)
        {
            // Arrange
            var submissionFromSession = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission;
            submissionFromSession.IsResubmission = true;
            submissionFromSession.SubmittedDate = DateTime.UtcNow;
            _facadeServiceMock.Setup(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null))
                .ReturnsAsync(new PomPayCalParametersResponse
                {
                    Reference = "12345",
                    MemberCount = 1,
                    ResubmissionDate = withResubmissionDate ? DateTime.UtcNow : null,
                    NationCode = "GB-ENG",
                });

            // Act
            var result = await _systemUnderTest.SubmissionDetails() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewName.Should().NotBeNull();
            result.ViewName.Should().Be(ViewName);

            var model = result!.Model as SubmissionDetailsViewModel;
            model!.OrganisationName.Should().Be(submissionFromSession.OrganisationName);
            model.SubmissionId.Should().Be(submissionFromSession.SubmissionId);
            model.SubmittedBy.Should().Be($"{submissionFromSession.FirstName} {submissionFromSession.LastName}");
            model.ReferenceFieldNotAvailable.Should().BeFalse();
            model.ReferenceNotAvailable.Should().BeFalse();
            AssertBackLink(result, PagePath.Submissions);
            _facadeServiceMock.Verify(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null), Times.Once);
        }
    }
}

