using EPR.RegulatorService.Frontend.Web.Validations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using FluentValidation.TestHelper;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validations;

[TestClass]
public class IdAndYearRequestValidatorTests
{
    private IdAndYearRequestValidator _validator;

    [TestInitialize]
    public void SetUp()
    {
        _validator = new IdAndYearRequestValidator();
    }

    [TestMethod]
    public void IsValid_WhenIdIsEmpty_ReturnsFalse()
    {
        var model = new IdAndYearRequest { Id = Guid.Empty, Year = 2025 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID must be a valid GUID.");
    }

    [TestMethod]
    public void IsValid_WhenYearIsBelow2000_ReturnsFalse()
    {
        var model = new IdAndYearRequest { Id = Guid.NewGuid(), Year = 1999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage("Year must be between 2000 and 2099.");
    }

    [TestMethod]
    public void IsValid_WhenYearIsAbove2099_ReturnsFalse()
    {
        var model = new IdAndYearRequest { Id = Guid.NewGuid(), Year = 2100 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage("Year must be between 2000 and 2099.");
    }

    [TestMethod]
    public void IsValid_WhenPassedValidData_ReturnsTrue()
    {
        var model = new IdAndYearRequest { Id = Guid.NewGuid(), Year = 2025 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
        result.ShouldNotHaveValidationErrorFor(x => x.Year);
    }
}
