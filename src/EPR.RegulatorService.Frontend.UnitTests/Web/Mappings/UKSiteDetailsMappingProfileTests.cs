using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class UKSiteDetailsMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UKSiteDetailsMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistration_ShouldReturnUkSiteDetailsViewModel()
    {
        // Arrange
        var ukSiteDetail = new UkSiteDetails
        {
            Id = 1,
            SiteAddress = "123 Test Street",
            Location = "England",
            LegalAddress = "321 Test Street",
            SiteGridReference = "SJ 854 662",
        };

        // Act
        var viewModel = _mapper.Map<UkSiteDetailsViewModel>(ukSiteDetail);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.RegistrationId.Should().Be(ukSiteDetail.Id);
            viewModel.Location.Should().Be(ukSiteDetail.Location);
            viewModel.SiteAddress.Should().Be(ukSiteDetail.SiteAddress);
            viewModel.LegalAddress.Should().Be(ukSiteDetail.LegalAddress);
            viewModel.SiteGridReference.Should().Be(ukSiteDetail.SiteGridReference);
        }
    }
}
