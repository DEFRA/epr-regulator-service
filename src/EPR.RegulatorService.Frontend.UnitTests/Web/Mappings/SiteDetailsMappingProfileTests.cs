using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class SiteDetailsMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SiteDetailsMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistration_ShouldReturnSiteDetailsViewModel()
    {
        // Arrange
        var siteDetails = new SiteDetails
        {
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Org",
            SiteAddress = "123 Test Street",
            NationName = "England",
            GridReference = "321 Test Street",
            LegalCorrespondenceAddress = "SJ 854 662",
        };

        // Act
        var viewModel = _mapper.Map<SiteDetailsViewModel>(siteDetails);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.RegistrationId.Should().Be(siteDetails.RegistrationId);
            viewModel.Location.Should().Be(siteDetails.NationName);
            viewModel.SiteAddress.Should().Be(siteDetails.SiteAddress);
            viewModel.LegalDocumentAddress.Should().Be(siteDetails.LegalCorrespondenceAddress);
            viewModel.SiteGridReference.Should().Be(siteDetails.GridReference);
            viewModel.TaskStatus.Should().Be(siteDetails.TaskStatus);
        }
    }
}
