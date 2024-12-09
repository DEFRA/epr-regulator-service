namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class GrantRegistrationSubmissionViewModelTests
    {
        [TestMethod]
        public void GrantRegistrationSubmissionViewModel_ValidModel_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new GrantRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsGrantRegistrationConfirmed = true
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
        }

        [TestMethod]
        public void GrantRegistrationSubmissionViewModel_ShouldHaveValidationError_WhenIsGrantRegistrationConfirmedIsNull()
        {
            // Arrange
            var viewModel = new GrantRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsGrantRegistrationConfirmed = null // Required field
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for null IsGrantRegistrationConfirmed.");
            Assert.AreEqual("ErrorMessage", validationResults[0].ErrorMessage, "Expected the correct error message for IsGrantRegistrationConfirmed being null.");
        }
    }
}
