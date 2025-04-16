namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validation;

using EPR.RegulatorService.Frontend.Web.Validations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation.TestHelper;

[TestClass]
public class IdRequestValidatorTests
{
    private IdRequestValidator _validator;

    [TestInitialize]
    public void SetUp()
    {
        _validator = new IdRequestValidator();
    }

    [TestMethod]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        // Arrange
        var model = new IdRequest { Id = 0 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID must be greater than 0.");
    }

    [TestMethod]
    public void Should_Have_Error_When_Id_Is_Negative()
    {
        // Arrange
        var model = new IdRequest { Id = -5 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID must be greater than 0.");
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_Id_Is_Positive()
    {
        // Arrange
        var model = new IdRequest { Id = 10 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}