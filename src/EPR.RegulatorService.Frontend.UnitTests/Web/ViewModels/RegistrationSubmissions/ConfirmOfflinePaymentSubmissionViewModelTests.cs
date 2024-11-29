namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class ConfirmOfflinePaymentSubmissionViewModelTests
    {
        [TestMethod]
        public void ConfirmOfflinePaymentSubmissionViewModel_ValidModel_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = "£100.00"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
        }

        [TestMethod]
        public void ConfirmOfflinePaymentSubmissionViewModel_NullSubmissionId_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = null, // Nullable field
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = "£100.00"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when SubmissionId is null.");
        }

        [TestMethod]
        public void ConfirmOfflinePaymentSubmissionViewModel_NullIsOfflinePaymentConfirmed_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsOfflinePaymentConfirmed = null, // Required field
                OfflinePaymentAmount = "£100.00"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for null IsOfflinePaymentConfirmed.");
            Assert.AreEqual("ConfirmOfflinePaymentSubmission.ErrorMessage", validationResults[0].ErrorMessage, "Expected the correct error message for IsOfflinePaymentConfirmed being null.");
        }

        [TestMethod]
        public void ConfirmOfflinePaymentSubmissionViewModel_OfflinePaymentAmountNullable_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = null // Nullable field
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when OfflinePaymentAmount is null.");
        }

        [TestMethod]
        public void ConfirmOfflinePaymentSubmissionViewModel_OfflinePaymentAmountEmpty_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = "" // Empty string
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when OfflinePaymentAmount is an empty string.");
        }
    }
}
