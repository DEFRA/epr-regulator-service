namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class ConfirmOfflinePaymentSubmissionViewModelTests
    {
        private readonly string _submissionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        [TestMethod]
        [DataRow("£100.00", DisplayName = "Valid amount")]
        [DataRow(null, DisplayName = "Null amount")]
        [DataRow("", DisplayName = "Empty amount")]
        public void ConfirmOfflinePaymentSubmissionViewModel_ValidModel_ShouldHaveNoValidationErrors(string offlinePaymentAmount)
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = _submissionId,
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = offlinePaymentAmount
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            validationResults.Should().BeEmpty("a valid model should not produce validation errors.");
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
            validationResults.Should().BeEmpty("SubmissionId is nullable, and null should not cause validation errors.");
        }

        [TestMethod]
        [DataRow(null, "ConfirmOfflinePaymentSubmission.ErrorMessage", DisplayName = "IsOfflinePaymentConfirmed is null")]
        public void ConfirmOfflinePaymentSubmissionViewModel_InvalidValues_ShouldHaveValidationError(
            bool? isOfflinePaymentConfirmed,
            string expectedErrorMessage)
        {
            // Arrange
            var viewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = _submissionId,
                IsOfflinePaymentConfirmed = isOfflinePaymentConfirmed,
                OfflinePaymentAmount = "£100.00"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            validationResults.Should().HaveCount(1, "there should be exactly one validation error for this scenario")
                .And.ContainSingle(result => result.ErrorMessage == expectedErrorMessage, "the error message should match the expected message.");
        }
    }
}
