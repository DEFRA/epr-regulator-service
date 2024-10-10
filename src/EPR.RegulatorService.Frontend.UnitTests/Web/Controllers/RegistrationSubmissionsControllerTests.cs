namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Web.Constants;
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
            string expectedBackLink = "/regulators/home";

            // Act
            var result = _controller.RegistrationSubmissions();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult.Model.GetType());

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange
            string expectedBacktoAllSubmissionsUrl = PagePath.RegistrationSubmissions;

            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                BackToAllSubmissionsUrl = expectedBacktoAllSubmissionsUrl
            };

            // Act
            var result = await _controller.QueryRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(QueryRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as QueryRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.Query, resultViewModel.Query);
            Assert.AreEqual(expectedViewModel.BackToAllSubmissionsUrl, resultViewModel.BackToAllSubmissionsUrl);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_SetsCorrectBackLinkInViewData()
        {
            // Act
            var result = await _controller.QueryRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel();
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_Redirects_WhenModelStateIsValid()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel();

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url);
        }
    }
}
