using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class ManageRegistrationsMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ManageRegistrationsMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void Should_Map_RegistrationDto_To_ManageRegistrationsViewModel_Correctly()
    {
        // Arrange
        var registrationDto = new Registration
        {
            Id = 1,
            OrganisationName = "Test Organisation",
            SiteAddress = "123 Test Street",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Custom Regulator"
        };

        // Act
        var viewModel = _mapper.Map<ManageRegistrationsViewModel>(registrationDto);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.Id.Should().Be(registrationDto.Id);
            viewModel.OrganisationName.Should().Be(registrationDto.OrganisationName);
            viewModel.SiteAddress.Should().Be(registrationDto.SiteAddress);
            viewModel.ApplicationOrganisationType.Should().Be(registrationDto.OrganisationType);
            viewModel.Regulator.Should().Be(registrationDto.Regulator);
        }
    }
}
