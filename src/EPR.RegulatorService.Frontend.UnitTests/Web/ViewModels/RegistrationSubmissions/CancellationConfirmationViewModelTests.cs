namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using System;
    using System.Collections.Generic;

    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class CancellationConfirmationViewModelTests
    {
        [TestMethod]
        public void CancellationConfirmationViewModel_ValidModel_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new CancellationConfirmationViewModel
            {
                SubmissionId = Guid.NewGuid(),
                OrganisationName = "Valid Organisation"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
        }

        [TestMethod]
        public void CancellationConfirmationViewModel_MissingSubmissionId_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new CancellationConfirmationViewModel
            {
                SubmissionId = null, // Nullable field is allowed
                OrganisationName = "Valid Organisation"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when SubmissionId is null.");
        }
    }
}
