using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using Frontend.Web.Configs;

    [TestClass]
    public class SubmissionDetailsTests : SubmissionsTestBase
    {
        private const string ViewName = "SubmissionDetails";
        private int _hashCode;
        private Mock<IFeatureManager> _mockFeatureManager;

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
                        PagePath.SubmissionDetails
                    },
                }
            };
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode] = testSubmission;
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            _mockFeatureManager = new Mock<IFeatureManager>();

            _mockFeatureManager.Setup(fm =>
                    fm.IsEnabledAsync(FeatureFlags.IncludeSubsidiariesInFeeCalculationsForRegulators))
                .ReturnsAsync(false);
        }

        [TestMethod]
        public async Task GivenOnSubmissionDetailsPage_WhenSubmissionDetailsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Arrange
            var submissionFromSession = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode];
            _facadeServiceMock.Setup(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null))
                .ReturnsAsync(new PomPayCalParametersResponse {
                    Reference = "12345",
                    MemberCount = 1,
                    ResubmissionDate = DateTime.UtcNow,
                    NationCode = "GB-ENG"
                });

            // Act
            var result = await _systemUnderTest.SubmissionDetails(_hashCode, _mockFeatureManager.Object) as ViewResult;

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
            model.SubmissionBlobName.Should().Be(submissionFromSession.PomBlobName);
            model.SubmissionFileName.Should().Be(submissionFromSession.PomFileName);
            model.ReferenceNumber.Should().Be(submissionFromSession.ReferenceNumber);
            model.NationCode.Should().Be(submissionFromSession.NationCode);
            model.MemberCount.Should().Be(submissionFromSession.MemberCount);
            AssertBackLink(result, PagePath.Submissions);
            _facadeServiceMock.Verify(r => r.GetPomPayCalParameters(It.IsAny<Guid>(), null), Times.Never);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task GivenOnSubmissionDetailsPage_WhenSubmissionDetailsHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session_Resubmission(bool withResubmissionDate)
        {
            // Arrange
            var submissionFromSession = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode];
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
            var result = await _systemUnderTest.SubmissionDetails(_hashCode, _mockFeatureManager.Object) as ViewResult;

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

