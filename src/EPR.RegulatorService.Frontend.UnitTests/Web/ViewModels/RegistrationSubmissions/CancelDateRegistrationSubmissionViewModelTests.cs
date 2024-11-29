namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using System;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;

    [TestClass]
    public class CancelDateRegistrationSubmissionViewModelTests
    {
        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_ValidFutureDate_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = 15,
                Month = 12,
                Year = 2025 // Future date
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid future cancellation date.");
            Assert.AreEqual(new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Unspecified), viewModel.CancellationDate);
        }

        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_InvalidPastDate_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = 15,
                Month = 10,
                Year = 2023 // Past date
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for a past cancellation date.");
            Assert.AreEqual("Error.PastDateErrorMessage", validationResults[0].ErrorMessage);
        }

        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_InvalidIncompleteDate_ShouldHaveValidationErrors()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = null, // Missing day
                Month = 12,
                Year = 2024
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for an incomplete date.");
            Assert.AreEqual("Error.MissingDayErrorMessage", validationResults[0].ErrorMessage);
        }

        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_InvalidRealDate_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = 31,
                Month = 2, // Invalid date
                Year = 2024
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for an invalid real date.");
            Assert.AreEqual("Error.InvalidRealDateErrorMessage", validationResults[0].ErrorMessage);
        }

        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_EmptyDate_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = null,
                Month = null,
                Year = null
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for an empty date.");
            Assert.AreEqual("Error.EmptyDateErrorMessage", validationResults[0].ErrorMessage);
        }

        [TestMethod]
        public void CancelDateRegistrationSubmissionViewModel_InvalidDayOutOfRange_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Day = 35, // Out of range
                Month = 12,
                Year = 2024
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for a day out of range.");
            Assert.AreEqual("Error.InvalidDateErrorMessage", validationResults[0].ErrorMessage);
        }
    }
}
