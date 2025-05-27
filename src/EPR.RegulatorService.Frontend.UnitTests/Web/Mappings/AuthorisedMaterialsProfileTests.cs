using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class AuthorisedMaterialsProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AuthorisedMaterialsProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistrationAuthorisedMaterials_ShouldReturnViewModel()
    {
        // Arrange
        var registration = new RegistrationAuthorisedMaterials
        {
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Organisation",
            SiteAddress = "123 Test Street",
            MaterialsAuthorisation = []
        };

        // Act
        var viewModel = _mapper.Map<AuthorisedMaterialsViewModel>(registration);

        // Assert
        using (new AssertionScope())
        {
            viewModel.RegistrationId.Should().Be(registration.RegistrationId);
            viewModel.OrganisationName.Should().Be(registration.OrganisationName);
            viewModel.SiteAddress.Should().Be(registration.SiteAddress);
            viewModel.TaskStatus.Should().Be(registration.TaskStatus);
        }
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistration_ShouldOrderMaterialsByIsRegisteredThenAlphabetically()
    {
        // Arrange
        var registration = new RegistrationAuthorisedMaterials
        {
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Organisation",
            SiteAddress = "123 Test Street",
            MaterialsAuthorisation = [
                CreateRegistrationMaterial(true, "ZZZ"),
                CreateRegistrationMaterial(false, "ZZZ"),
                CreateRegistrationMaterial(true, "AAA"),
                CreateRegistrationMaterial(false, "AAA"),
            ]
        };

        // Act
        var viewModel = _mapper.Map<AuthorisedMaterialsViewModel>(registration);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Materials.Count.Should().Be(4);

            viewModel.Materials[0].IsMaterialRegistered.Should().Be(true);
            viewModel.Materials[1].IsMaterialRegistered.Should().Be(true);
            viewModel.Materials[2].IsMaterialRegistered.Should().Be(false);
            viewModel.Materials[3].IsMaterialRegistered.Should().Be(false);

            viewModel.Materials[0].MaterialName.Should().Be("AAA");
            viewModel.Materials[1].MaterialName.Should().Be("ZZZ");
            viewModel.Materials[2].MaterialName.Should().Be("AAA");
            viewModel.Materials[3].MaterialName.Should().Be("ZZZ");
        }
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistrationMaterial_ShouldReturnViewModel()
    {
        // Arrange
        var registrationMaterial = new MaterialsAuthorisedOnSite
        {
            MaterialName = "Plastic",
            IsMaterialRegistered = true,
            Reason = "Some reason",
        };

        // Act
        var viewModel = _mapper.Map<AuthorisedMaterialViewModel>(registrationMaterial);

        // Assert
        using (new AssertionScope())
        {
            viewModel.IsMaterialRegistered.Should().Be(registrationMaterial.IsMaterialRegistered);
            viewModel.MaterialName.Should().Be(registrationMaterial.MaterialName);
            viewModel.Reason.Should().Be(registrationMaterial.Reason);
        }
    }

    private static MaterialsAuthorisedOnSite CreateRegistrationMaterial(bool isMaterialRegistered, string materialName) =>
        new()
        {
            MaterialName = materialName, IsMaterialRegistered = isMaterialRegistered
        };
}