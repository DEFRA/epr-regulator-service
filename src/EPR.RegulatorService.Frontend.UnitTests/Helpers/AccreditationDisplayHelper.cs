namespace EPR.RegulatorService.Frontend.UnitTests.Helpers;

using System;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

[TestClass]
public class AccreditationDisplayHelperTests
{
    [TestMethod]
    public void ShouldDisplayMaterial_ReturnsFalse_WhenRegistrationStatusIsNull()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayMaterial(null, new AccreditationDetailsViewModel
        {
            Status = ApplicationStatus.Granted
        });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldDisplayMaterial_ReturnsFalse_WhenAccreditationIsNull()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayMaterial(ApplicationStatus.Granted, null);
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldDisplayMaterial_ReturnsFalse_WhenAccreditationStatusIsInactive()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayMaterial(ApplicationStatus.Granted, new AccreditationDetailsViewModel
        {
            Status = ApplicationStatus.Withdrawn
        });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldDisplayMaterial_ReturnsTrue_WhenBothAreValid()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayMaterial(ApplicationStatus.Granted, new AccreditationDetailsViewModel
        {
            Status = ApplicationStatus.Granted
        });

        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldDisplayAccreditation_ReturnsFalse_WhenStatusIsWithdrawn()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayAccreditation(ApplicationStatus.Withdrawn);
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldDisplayAccreditation_ReturnsTrue_WhenStatusIsGranted()
    {
        var result = AccreditationDisplayHelper.ShouldDisplayAccreditation(ApplicationStatus.Granted);
        result.Should().BeTrue();
    }

}
