namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using System.ComponentModel.DataAnnotations;

    [TestClass]
    public class QueryRegistrationSubmissionViewModelTests
    {
        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [TestMethod]
        public void QueryRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_ForAValidModel()
        {
            // Arrange
            var viewModel = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Query = "This is a valid query within the allowed length."
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
        }

        [TestMethod]
        public void QueryRegistrationSubmissionViewModel_ShouldHaveValidationError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var viewModel = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                Query = new string('A', 401) // Exceeding the 400-character limit
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected a validation error for an excessively long query.");
            Assert.AreEqual("Error.QueryTooLong", validationResults[0].ErrorMessage);
        }

        [TestMethod]
        public void QueryRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_WhenSubmissionIdIsMissing()
        {
            // Arrange
            var viewModel = new QueryRegistrationSubmissionViewModel
            {
                Query = "Valid query text"
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "SubmissionId is not required, so the model should still be valid.");
        }
    }
}
