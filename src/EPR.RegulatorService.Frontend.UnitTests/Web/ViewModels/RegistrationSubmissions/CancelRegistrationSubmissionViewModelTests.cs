namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class CancelRegistrationSubmissionViewModelTests
{
    private readonly string _submissionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    [TestMethod]
    public void CancelRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_ForValidModel()
    {
        // Arrange
        var viewModel = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = _submissionId,
            CancellationReason = "This is a valid cancellation reason within the limit of 400 characters."
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
    }

    [TestMethod]
    public void CancelRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_ForANullCancellationReason()
    {
        // Arrange
        var viewModel = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = _submissionId,
            CancellationReason = null // Nullable field
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when CancellationReason is null.");
    }

    [TestMethod]
    public void CancelRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_ForACancellationReasonWithWhitespace()
    {
        // Arrange
        var viewModel = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = _submissionId,
            CancellationReason = "     Valid cancellation reason."
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when CancellationReason contains whitespace.");
    }

    [TestMethod]
    public void CancelRegistrationSubmissionViewModel_ShouldHaveNoValidationErrors_ForAnEmptyCancellationReason()
    {
        // Arrange
        var viewModel = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = _submissionId,
            CancellationReason = "" // Empty string
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for an empty CancellationReason.");
    }

    [TestMethod]
    public void CancelRegistrationSubmissionViewModel_ShouldHaveValidationError_WhenCancellationReasonTooLong()
    {
        // Arrange
        var viewModel = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = _submissionId,
            CancellationReason = new string('A', 401) // Exceeds 400 characters
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for a CancellationReason exceeding 400 characters.");
        Assert.AreEqual("Error.CancellationReasonTooLong", validationResults[0].ErrorMessage, "Expected the correct error message for CancellationReason too long.");
    }
}