namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;

    [TestClass]
    public class RegistrationSubmissionsControllerTests : RegistrationSubmissionsTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel()
        {
            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel();

            // Act
            var result = _controller.RegistrationSubmissions();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult.Model.GetType());
        }
    }
}
