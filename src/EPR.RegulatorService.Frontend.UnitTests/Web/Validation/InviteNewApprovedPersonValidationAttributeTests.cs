namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validation;

using System.ComponentModel.DataAnnotations;

using Frontend.Web.ViewModels.InviteNewApprovedPerson;

[TestClass]
public class InviteNewApprovedPersonValidationAttributeTests
{
    [TestMethod]
    [DataRow(null)]
    public void EnterPersonNameModel_WhenPassedNull_ReturnsNullValidationResult(int searchTermLength)
    {
        //Arrange
        var viewModel = new EnterPersonNameModel {
            FirstName = "",
            LastName = ""};

        // Act
        var validationResults = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);
        var requiredError = validationResults[0];
        
        // Assert
        Assert.AreEqual("ErrorTitle.FirstNameNull", requiredError.ErrorMessage);
        Assert.AreEqual(isValid,false);
    }

    [TestMethod]
    public void EnterPersonNameModel_WhenPassedNumber_ReturnsValidationResult()
    {
        // Arrange
        var viewModel = new EnterPersonNameModel {
            FirstName = "123213",
            LastName = "2344324" };
        var validationResults = new List<ValidationResult>();

        // Act
        Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);

        // Assert
        var requiredError = validationResults[0];
        Assert.AreEqual("ErrorTitle.FirstNameDigits", requiredError.ErrorMessage);
    }

    [TestMethod]
    [DataRow(255, "TooLongError")]
    [DataRow(256, "TooLongError")]
    public void EnterPersonNameModel_WhenNameIsInvalidLength_ReturnsValidationResult(int searchTermLength, string errorMessage)
    {
        //Arrange
        var viewModel = new EnterPersonNameModel {
            FirstName = new string('X', searchTermLength),
            LastName = new string('Y', searchTermLength) };
        var validationResults = new List<ValidationResult>();
        
        // Act
        Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);

        // Assert
        var requiredError = validationResults[0];
        Assert.AreEqual("ErrorTitle.FirstNameLength", requiredError.ErrorMessage);
    }
    
    [TestMethod]
    [DataRow(null)]
    public void EnterPersonEmailModel_WhenPassedNull_ReturnsNullValidationResult(int searchTermLength)
    {
        //Arrange
        var viewModel = new EnterPersonEmailModel {
            Email = ""};

        // Act
        var validationResults = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);
        var requiredError = validationResults[0];
        
        // Assert
        Assert.AreEqual("PersonEmail.MissingError", requiredError.ErrorMessage);
        Assert.AreEqual(isValid,false);
    }

    [TestMethod]
    public void EnterPersonEmailModel_WhenPassedInvalidFormat_ReturnsValidationResult()
    {
        // Arrange
        var viewModel = new EnterPersonEmailModel {
            Email = "a"};
        var validationResults = new List<ValidationResult>();

        // Act
        Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);

        // Assert
        var requiredError = validationResults[0];
        Assert.AreEqual("PersonEmail.InvalidFormat", requiredError.ErrorMessage);
    }

    [TestMethod]
    [DataRow(255, "TooLongError")]
    [DataRow(256, "TooLongError")]
    public void EnterPersonEmailModel_WhenNameIsInvalidLength_ReturnsValidationResult(int searchTermLength, string errorMessage)
    {
        //Arrange
        var viewModel = new EnterPersonEmailModel {
            Email = "@" + new string('X', searchTermLength) + ".com"};
        var validationResults = new List<ValidationResult>();
        
        // Act
        Validator.TryValidateObject(viewModel, new ValidationContext(viewModel), validationResults, true);

        // Assert
        var requiredError = validationResults[0];
        Assert.AreEqual("PersonEmail.TooLongError", requiredError.ErrorMessage);
    }
}