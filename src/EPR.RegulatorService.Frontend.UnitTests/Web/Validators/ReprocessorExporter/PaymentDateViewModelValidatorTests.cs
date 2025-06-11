using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Validations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

using FluentValidation.TestHelper;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validators.ReprocessorExporter;

[TestClass]
public class PaymentDateViewModelValidatorTests
{
    private PaymentDateViewModelValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _validator = new PaymentDateViewModelValidator();
    }

    [TestMethod]
    public void Should_Have_Error_When_Day_Is_Null1()
    {
        var model = new PaymentDateViewModel { Day = null, Month = 5, Year = 2023, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Day)
              .WithErrorMessage("Enter date in DD MM YYYY format");
    }

    [TestMethod]
    public void Should_Have_Error_When_Day_Is_Out_Of_Range()
    {
        var model = new PaymentDateViewModel { Day = 32, Month = 5, Year = 2023, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Day)
              .WithErrorMessage("Enter a valid day");
    }

    [TestMethod]
    public void Should_Have_Error_When_Month_Is_Null()
    {
        var model = new PaymentDateViewModel { Day = 5, Month = null, Year = 2023, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Month)
              .WithErrorMessage("Enter date in DD MM YYYY format");
    }

    [TestMethod]
    public void Should_Have_Error_When_Month_Is_Out_Of_Range()
    {
        var model = new PaymentDateViewModel { Day = 5, Month = 13, Year = 2023, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Month)
              .WithErrorMessage("Enter a valid month");
    }

    [TestMethod]
    public void Should_Have_Error_When_Year_Is_Null()
    {
        var model = new PaymentDateViewModel { Day = 5, Month = 5, Year = null, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage("Enter a valid year in YYYY format");
    }

    [TestMethod]
    public void Should_Have_Error_When_Year_Is_Out_Of_Range()
    {
        var model = new PaymentDateViewModel { Day = 5, Month = 5, Year = 1999, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Year)
              .WithErrorMessage("Enter a valid year in YYYY format");
    }

    [TestMethod]
    public void Should_Have_Error_When_Date_Is_Invalid()
    {
        var model = new PaymentDateViewModel { Day = 31, Month = 2, Year = 2023, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("")
              .WithErrorMessage("Enter a valid date");
    }

    [TestMethod]
    public void Should_Have_Error_When_Date_Is_In_Future()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var model = new PaymentDateViewModel
        {
            Day = futureDate.Day,
            Month = futureDate.Month,
            Year = futureDate.Year,
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("")
              .WithErrorMessage("Date must be either today or in the past");
    }

    [TestMethod]
    public void Should_Pass_For_Valid_Past_Date()
    {
        var model = new PaymentDateViewModel { Day = 1, Month = 1, Year = 2020, ApplicationType = ApplicationOrganisationType.Reprocessor };
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void Should_Pass_For_Todays_Date()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var model = new PaymentDateViewModel
        {
            Day = today.Day,
            Month = today.Month,
            Year = today.Year,
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }
}
