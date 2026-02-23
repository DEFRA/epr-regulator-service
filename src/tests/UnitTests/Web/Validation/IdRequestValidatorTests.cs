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
    public void Should_Have_Error_When_Id_Is_Empt()
    {
        // Arrange
        var model = new IdRequest { Id = Guid.Empty };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID is required.");
    }
}