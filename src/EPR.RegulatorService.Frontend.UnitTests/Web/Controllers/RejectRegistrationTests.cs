namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using Frontend.Core.Models;
    using Frontend.Core.Models.Registrations;
    using Frontend.Core.Sessions;
    using Frontend.Web.Constants;
    using Frontend.Web.Controllers.Registrations;
    using Frontend.Web.ViewModels.Registrations;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using Moq;

    [TestClass]
    public class RejectRegistrationTests : RegistrationTestBase
    {
        private const string RejectRegistrationViewName = "RejectRegistration";
        private const string OrganisationName = "Test Organisation";
        private const string ModelError = "Test Model Error";
        private readonly Guid _submissionId = Guid.NewGuid();
        private readonly Guid _fileId = Guid.NewGuid();

        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            JourneySessionMock = new JourneySession
            {
                RegulatorRegistrationSession = new RegulatorRegistrationSession
                {
                    Journey = new List<string>
                    {
                        PagePath.Registrations, PagePath.RegistrationDetails, PagePath.RejectRegistration
                    },
                    OrganisationRegistration = new Registration
                    {
                        OrganisationDetailsFileId = _fileId,
                        OrganisationName = OrganisationName,
                        SubmissionId = _submissionId
                    }
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task RejectRegistrationGet_WhenAccessedFromRegistrationDetails_SetsBackLinkCorrectly()
        {
            // Act
            var viewResult = await _sut.RejectRegistration() as ViewResult;

            // Assert
            Assert.IsNotNull(viewResult);
            viewResult.ViewName.Should().Be(RejectRegistrationViewName);
            AssertBackLink(viewResult, PagePath.RegistrationDetails);
        }

        [TestMethod]
        public async Task RejectRegistrationPost_WhenModelInvalid_CreatesValidationError()
        {

            // Arrange
            var viewModel = new RejectRegistrationViewModel()
            {
                ReasonForRejection = null,
            };

            _sut.ModelState.AddModelError(ModelErrorKey, ModelError);

            _facadeServiceMock.Setup(x => x.SubmitRegistrationDecision(It.IsAny<RegulatorRegistrationDecisionCreateRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _sut.RejectRegistration(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(RejectRegistrationViewName);
            Assert.AreEqual(
                expected: result.ViewData.ModelState[ModelErrorKey]!.Errors[0].ErrorMessage,
                actual: ModelError
            );

            _facadeServiceMock.Verify(x => x.SubmitRegistrationDecision(It.IsAny<RegulatorRegistrationDecisionCreateRequest>()),
                Times.Never);
        }

        [TestMethod]
        public async Task RejectRegistrationPost_WhenValidModelAndSession_CreatesDecisionAndRedirects()
        {
            // Arrange
            var viewModel = new RejectRegistrationViewModel()
            {
                ReasonForRejection = string.Empty,
            };

            _facadeServiceMock.Setup(x => x.SubmitRegistrationDecision(It.IsAny<RegulatorRegistrationDecisionCreateRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _sut.RejectRegistration(viewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            result.ActionName.Should().Be(nameof(RegistrationsController.Registrations));

            var routeValues = result.RouteValues;
            Assert.IsNotNull(routeValues);
            routeValues.Values.Should().Contain(EndpointResponseStatus.Success);
            routeValues.Values.Should().Contain(OrganisationName);

            _facadeServiceMock.Verify(x => x.SubmitRegistrationDecision(It.IsAny<RegulatorRegistrationDecisionCreateRequest>()),
                Times.Once);
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()),
                Times.Once);
        }
    }
}