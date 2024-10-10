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

        #region RegistrationSubmissions

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

        #endregion

        #region QueryRegistrationSubmission

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
        public async Task QueryRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenQueryIsValid()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = "Valid query within 400 characters." // Valid input
            };

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = new string('A', 401) // 401 characters
            };

            // Add a validation rule that the maximum length is 400 for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.Query), "Reason for querying application must be 400 characters or less");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)]?.Errors.Count > 0);
            Assert.AreEqual("Reason for querying application must be 400 characters or less",
                _controller.ModelState[nameof(model.Query)]?.Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenNoQueryIsProvided()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = null // No query provided
            };

            // Simulate the model validation error for required Query field
            _controller.ModelState.AddModelError(nameof(model.Query), "Enter the reason you are querying this registration application");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)]?.Errors.Count > 0);
            Assert.AreEqual("Enter the reason you are querying this registration application",
                _controller.ModelState[nameof(model.Query)]?.Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        #endregion

        #region RejectRegistrationSubmission

        [TestMethod]
        public async Task RejectRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange
            string expectedBacktoAllSubmissionsUrl = PagePath.RegistrationSubmissions;

            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                BackToAllSubmissionsUrl = expectedBacktoAllSubmissionsUrl
            };

            // Act
            var result = await _controller.RejectRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(RejectRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as RejectRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.RejectReason, resultViewModel.RejectReason);
            Assert.AreEqual(expectedViewModel.BackToAllSubmissionsUrl, resultViewModel.BackToAllSubmissionsUrl);
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_SetsCorrectBackLinkInViewData()
        {
            // Act
            var result = await _controller.RejectRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel();
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenRejectionReasonIsValid()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = "Valid rejection reason within 400 characters." // Valid input
            };

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = new string('A', 401) // 401 characters
            };

            // Add a validation rule that the maximum length is 400 for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Reason for rejecting application must be 400 characters or less");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)]?.Errors.Count > 0);
            Assert.AreEqual("Reason for rejecting application must be 400 characters or less",
                _controller.ModelState[nameof(model.RejectReason)]?.Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenNoRejectionReasonIsProvided()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = null // No rejection reason provided
            };

            // Simulate the model validation error for required RejectionReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Enter the reason you are rejecting this registration application");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)]?.Errors.Count > 0);
            Assert.AreEqual("Enter the reason you are rejecting this registration application",
                _controller.ModelState[nameof(model.RejectReason)]?.Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        #endregion
    }
}
