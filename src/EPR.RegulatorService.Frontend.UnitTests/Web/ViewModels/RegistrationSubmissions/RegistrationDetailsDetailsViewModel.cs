namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class RegistrationSubmissionDetailsViewModelTests
{
    [TestMethod]
    public void ImplicitOperator_ShouldConvert_RegistrationSubmissionOrganisationDetails_To_RegistrationSubmissionDetailsViewModel()
    {
        // Arrange
        var details = new RegistrationSubmissionOrganisationDetails
        {
            OrganisationId = Guid.NewGuid(),
            OrganisationReference = "ORGREF1234567890",
            OrganisationName = "Test Organisation",
            ApplicationReferenceNumber = "APPREF123",
            RegistrationReferenceNumber = "REGREF456",
            OrganisationType = RegistrationSubmissionOrganisationType.large,
            CompaniesHouseNumber = "CH123456",
            SubmissionStatus = RegistrationSubmissionStatus.Pending,
            SubmissionDate = new DateTime(2023, 4, 23, 0, 0, 0, DateTimeKind.Unspecified),
            BuildingName = "Building A",
            SubBuildingName = "Sub A",
            BuildingNumber = "123",
            Street = "Test Street",
            Town = "Test Town",
            County = "Test County",
            Country = "Test Country",
            Postcode = "TC1234",
        };

        // Act
        RegistrationSubmissionDetailsViewModel viewModel = details;

        // Assert
        Assert.AreEqual(details.OrganisationId, viewModel.OrganisationId);
        Assert.AreEqual(details.OrganisationReference[..10], viewModel.OrganisationReference);
        Assert.AreEqual(details.OrganisationName, viewModel.OrganisationName);
        Assert.AreEqual(details.ApplicationReferenceNumber, viewModel.ApplicationReferenceNumber);
        Assert.AreEqual(details.RegistrationReferenceNumber, viewModel.RegistrationReferenceNumber);
        Assert.AreEqual(details.OrganisationType, viewModel.OrganisationType);
        Assert.AreEqual(details.CompaniesHouseNumber, viewModel.CompaniesHouseNumber);
        Assert.AreEqual(details.Country, viewModel.RegisteredNation);
        Assert.AreEqual(details.SubmissionStatus, viewModel.Status);
        Assert.AreEqual(details.SubmissionDate, viewModel.RegistrationDateTime);

        // Check Business Address mapping
        Assert.AreEqual(details.BuildingName, viewModel.BusinessAddress.BuildingName);
        Assert.AreEqual(details.SubBuildingName, viewModel.BusinessAddress.SubBuildingName);
        Assert.AreEqual(details.BuildingNumber, viewModel.BusinessAddress.BuildingNumber);
        Assert.AreEqual(details.Street, viewModel.BusinessAddress.Street);
        Assert.AreEqual(details.Town, viewModel.BusinessAddress.Town);
        Assert.AreEqual(details.County, viewModel.BusinessAddress.County);
        Assert.AreEqual(details.Country, viewModel.BusinessAddress.Country);
        Assert.AreEqual(details.Postcode, viewModel.BusinessAddress.PostCode);
    }

    [TestMethod]
    public void ViewModel_ShouldHave_AllPropertiesSetCorrectly()
    {
        // Arrange
        var viewModel = new RegistrationSubmissionDetailsViewModel
        {
            OrganisationId = Guid.NewGuid(),
            OrganisationReference = "ORGREF1234",
            OrganisationName = "Test Organisation",
            ApplicationReferenceNumber = "APP123",
            RegistrationReferenceNumber = "REG123",
            OrganisationType = RegistrationSubmissionOrganisationType.small,
            CompaniesHouseNumber = "CH987654",
            RegisteredNation = "Test Country",
            PowerBiLogin = "testlogin@org.com",
            Status = RegistrationSubmissionStatus.Granted,
            RegistrationDateTime = new DateTime(2023, 4, 23, 0, 0, 0, DateTimeKind.Unspecified),
            BusinessAddress = new BusinessAddress
            {
                BuildingName = "Building B",
                SubBuildingName = "Sub B",
                BuildingNumber = "456",
                Street = "Example Street",
                Town = "Example Town",
                County = "Example County",
                Country = "Example Country",
                PostCode = "EX1234"
            }
        };

        // Act & Assert
        Assert.IsNotNull(viewModel.OrganisationId);
        Assert.AreEqual("ORGREF1234", viewModel.OrganisationReference);
        Assert.AreEqual("Test Organisation", viewModel.OrganisationName);
        Assert.AreEqual("APP123", viewModel.ApplicationReferenceNumber);
        Assert.AreEqual("REG123", viewModel.RegistrationReferenceNumber);
        Assert.AreEqual(RegistrationSubmissionOrganisationType.small, viewModel.OrganisationType);
        Assert.AreEqual("CH987654", viewModel.CompaniesHouseNumber);
        Assert.AreEqual("Test Country", viewModel.RegisteredNation);
        Assert.AreEqual("testlogin@org.com", viewModel.PowerBiLogin);
        Assert.AreEqual(RegistrationSubmissionStatus.Granted, viewModel.Status);
        Assert.AreEqual(new DateTime(2023, 4, 23, 0, 0, 0, DateTimeKind.Unspecified), viewModel.RegistrationDateTime);

        // Assert BusinessAddress
        Assert.AreEqual("Building B", viewModel.BusinessAddress.BuildingName);
        Assert.AreEqual("Sub B", viewModel.BusinessAddress.SubBuildingName);
        Assert.AreEqual("456", viewModel.BusinessAddress.BuildingNumber);
        Assert.AreEqual("Example Street", viewModel.BusinessAddress.Street);
        Assert.AreEqual("Example Town", viewModel.BusinessAddress.Town);
        Assert.AreEqual("Example County", viewModel.BusinessAddress.County);
        Assert.AreEqual("Example Country", viewModel.BusinessAddress.Country);
        Assert.AreEqual("EX1234", viewModel.BusinessAddress.PostCode);
    }

    [TestMethod]
    public void ImplicitOperator_Handles_EmptyStringsCorrectly()
    {
        // Arrange
        var details = new RegistrationSubmissionOrganisationDetails
        {
            OrganisationId = Guid.NewGuid(),
            OrganisationReference = "ORGREF1234567890",
            OrganisationName = "Test Organisation",
            ApplicationReferenceNumber = string.Empty,
            RegistrationReferenceNumber = string.Empty,
            OrganisationType = RegistrationSubmissionOrganisationType.large,
            CompaniesHouseNumber = "CH123456",
            SubmissionStatus = RegistrationSubmissionStatus.Cancelled,
            SubmissionDate = new DateTime(2023, 5, 10, 0, 0, 0, DateTimeKind.Unspecified),
            BuildingName = null,
            SubBuildingName = null,
            BuildingNumber = null,
            Street = "Test Street",
            Town = "Test Town",
            County = "Test County",
            Country = "Test Country",
            Postcode = "TC1234"
        };

        // Act
        RegistrationSubmissionDetailsViewModel viewModel = details;

        // Assert
        Assert.AreEqual(string.Empty, viewModel.ApplicationReferenceNumber);
        Assert.AreEqual(string.Empty, viewModel.RegistrationReferenceNumber);
        Assert.IsNull(viewModel.BusinessAddress.BuildingName);
        Assert.IsNull(viewModel.BusinessAddress.SubBuildingName);
        Assert.IsNull(viewModel.BusinessAddress.BuildingNumber);
        Assert.AreEqual("Test Street", viewModel.BusinessAddress.Street);
    }
}
