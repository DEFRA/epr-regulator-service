using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;

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

            AssertBackLink(result, PagePath.Submissions);
        }
    }
}

