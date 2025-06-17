using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class WasteCarrierMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<WasteCarrierMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithModel_ShouldReturnViewModel()
    {
        // Arrange
        var wasteCarrierDetails = new WasteCarrierDetails
        {
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Org",
            SiteAddress = "123 Test Street",
            WasteCarrierBrokerDealerNumber = "123456789",
            TaskStatus = RegulatorTaskStatus.Completed
        };

        // Act
        var viewModel = _mapper.Map<WasteCarrierDetailsViewModel>(wasteCarrierDetails);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.RegistrationId.Should().Be(wasteCarrierDetails.RegistrationId);
            viewModel.OrganisationName.Should().Be(wasteCarrierDetails.OrganisationName);
            viewModel.SiteAddress.Should().Be(wasteCarrierDetails.SiteAddress);
            viewModel.WasteCarrierBrokerDealerNumber.Should().Be(wasteCarrierDetails.WasteCarrierBrokerDealerNumber);
            viewModel.TaskStatus.Should().Be(wasteCarrierDetails.TaskStatus);
        }
    }

    [TestMethod]
    public void Map_WhenCalledWithModel_ShouldReturnQueryRegistrationSession()
    {
        // Arrange
        var wasteCarrierDetails = new WasteCarrierDetails
        {
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Org",
            SiteAddress = "123 Test Street",
            WasteCarrierBrokerDealerNumber = "123456789",
            TaskStatus = RegulatorTaskStatus.Completed,
            RegulatorRegistrationTaskStatusId = Guid.NewGuid()
        };

        // Act
        var result = _mapper.Map<QueryRegistrationSession>(wasteCarrierDetails);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be(wasteCarrierDetails.OrganisationName);
        result.SiteAddress.Should().Be(wasteCarrierDetails.SiteAddress);
        result.RegistrationId.Should().Be(wasteCarrierDetails.RegistrationId);
        result.RegulatorRegistrationTaskStatusId.Should().Be(wasteCarrierDetails.RegulatorRegistrationTaskStatusId.Value);
        result.PagePath.Should().BeNull();
    }
}