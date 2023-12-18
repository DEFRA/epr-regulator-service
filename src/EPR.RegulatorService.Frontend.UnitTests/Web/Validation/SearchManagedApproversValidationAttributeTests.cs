using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;

using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validation;

[TestClass]
public class SearchManagedApproversValidationAttributeTests
{
    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(80)]
    [DataRow(150)]
    [DataRow(252)]
    [DataRow(253)]
    [DataRow(254)]
    public void SearchTermModel_WhenPassedValidNumber_ReturnsNullValidationResult(int searchTermLength)
    {
        //Arrange
        var viewModel = new SearchTermViewModel { SearchTerm = new string('X', searchTermLength) };

        // Act
        var validationResults = viewModel.Validate(new ValidationContext(viewModel));

        // Assert
        Assert.IsNull(validationResults.FirstOrDefault());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("     ")]
    [DataRow(null)]
    public void SearchTermModel_WhenSearchTermIsEmpty_ReturnsValidationResult(string value)
    {
        // Arrange
        var viewModel = new SearchTermViewModel { SearchTerm = value };
        var validationResults = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);

        // Assert
        Assert.IsFalse(isValid, "Model should be invalid");
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error");

        var requiredError = validationResults[0];
        Assert.AreEqual("SearchTerm.MissingError", requiredError.ErrorMessage);
    }

    [TestMethod]
    [DataRow(1, "TooShortError")]
    [DataRow(2, "TooShortError")]
    [DataRow(255, "TooLongError")]
    [DataRow(256, "TooLongError")]
    public void SearchTermModel_WhenSearchTermIsInvalidLength_ReturnsValidationResult(int searchTermLength, string errorMessage)
    {
        //Arrange
        var viewModel = new SearchTermViewModel { SearchTerm = new string('X', searchTermLength) };

        // Act
        var validationResults = viewModel.Validate(new ValidationContext(viewModel)).ToList();

        // Assert
        Assert.AreEqual(2, validationResults.Count, "Expected two validation errors");

        var orgNameError = validationResults[0];
        Assert.AreEqual($"SearchTerm.OrganisationName.{errorMessage}", orgNameError.ErrorMessage);

        var orgIdError = validationResults[1];
        Assert.AreEqual($"SearchTerm.OrganisationId.{errorMessage}", orgIdError.ErrorMessage);
    }
}