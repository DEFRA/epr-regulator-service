namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Core.Models.Submissions;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.UnitTests.TestData;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

    [TestClass]
    public class MultiTabSessionTests : SubmissionsTestBase
    {
        private const string ViewName = "AcceptSubmission";
        private Dictionary<int, Submission> _organisationSubmissions;


        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            var testSubmission1 = TestSubmission.GetTestSubmission("Sub1");
            var testSubmission2 = TestSubmission.GetTestSubmission("Sub2");
            var testSubmission3 = TestSubmission.GetTestSubmission("Sub3");
            var testSubmission4 = TestSubmission.GetTestSubmission("Sub4");
            int hashCode1 = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission1);
            int hashCode2 = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission2);
            int hashCode3 = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission3);
            int hashCode4 = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission4);

            _organisationSubmissions = new Dictionary<int, Submission>
            {

                {hashCode1,testSubmission1 },
                {hashCode2,testSubmission2 },
                {hashCode3,testSubmission3 },
                {hashCode4,testSubmission4 },
            };

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

            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions = _organisationSubmissions;
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task MultiTab_GivenOnAcceptSubmissionPage_WhenAcceptSubmissionHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            foreach (var kvp in _organisationSubmissions)
            {
                int hashCode = kvp.Key;

                // Arrange

                var organisationSubmission = JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmissions[hashCode];
                string expectedBackLink = $"/regulators/{PagePath.SubmissionDetails}?submissionHash={hashCode}";

                // Act
                var result = await _systemUnderTest.AcceptSubmission(hashCode) as ViewResult;

                // Assert
                result.Should().NotBeNull();
                result!.Model.Should().NotBeNull();
                result.ViewName.Should().Be(ViewName);

                var model = result.Model as AcceptSubmissionViewModel;
                model!.OrganisationName.Should().Be(organisationSubmission.OrganisationName);
                AssertBackLink(result, expectedBackLink);
            }
        }
    }
}
