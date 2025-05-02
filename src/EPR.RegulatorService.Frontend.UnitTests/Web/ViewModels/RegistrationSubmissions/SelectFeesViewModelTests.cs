using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class SelectFeesViewModelTests
{
    [TestMethod]
    public void SelectFeesViewModel_ApplicationFeeInPounds_ShouldFormatCorrectly()
    {
        // Arrange
        SelectFeesViewModel viewModel = new() { ApplicationFee = 123.45m };

        // Act
        string result = viewModel.ApplicationFeeInPounds;

        // Assert
        result.Should().Be("£123.45");
    }

    [TestMethod]
    public void SelectFeesViewModel_ApplicationFeeSectionEnable_WhenApplicationFeeIsGreaterThanZero_ShouldReturnTrue()
    {
        // Arrange
        SelectFeesViewModel viewModel = new() { ApplicationFee = 10m };

        // Act
        bool result = viewModel.ApplicationFeeSectionEnable;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void SelectFeesViewModel_ApplicationFeeSectionEnable_WhenApplicationFeeIsZero_ShouldReturnFalse()
    {
        // Arrange
        SelectFeesViewModel viewModel = new() { ApplicationFee = 0m };

        // Act
        bool result = viewModel.ApplicationFeeSectionEnable;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenNoFeeIsChecked_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new();

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Select a fee to waive");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedComplianceSchemeAmountExceedsApplicationFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsComplianceSchemeChecked = true,
            ApplicationFee = 100m,
            WavedComplianceSchemeAmount = 150m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedSmallProducerFeeExceedsSmallProducerFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsSmallProducerChecked = true,
            SmallProducerFee = 50m,
            WavedSmallProducerFee = 60m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedLargeProducerFeeExceedsLargeProducerFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsLargeProducerChecked = true,
            LargeProducerFee = 200m,
            WavedLargeProducerFee = 250m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedOnlineMarketPlaceFeeExceedsOnlineMarketPlaceFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsOnineMarketPlaceChecked = true,
            OnlineMarketPlaceFee = 300m,
            WavedOnlineMarketPlaceFee = 350m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedSubsidiariesCompanyFeeExceedsSubsidiariesCompanyFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsSubsidiariesCompanyChecked = true,
            SubsidiariesCompanyFee = 400m,
            WavedSubsidiariesCompanyFee = 450m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    [TestMethod]
    public void SelectFeesViewModel_WhenWavedLateProducerFeeExceedsLateProducerFee_ShouldReturnError()
    {
        // Arrange
        SelectFeesViewModel viewModel = new()
        {
            IsLateProducerChecked = true,
            LateProducerFee = 500m,
            WavedLateProducerFee = 550m
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        results.Should().ContainSingle(result => result.ErrorMessage == "Waved amount cannot be more than actual amount");
    }

    private static IList<ValidationResult> ValidateModel(SelectFeesViewModel viewModel)
    {
        ValidationContext validationContext = new(viewModel);
        List<ValidationResult> results = new();
        Validator.TryValidateObject(viewModel, validationContext, results, true);
        return results;
    }
}
