using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

using Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

[TestClass]
public class ApplicationUpdateMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationUpdateMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithRegistrationMaterial_ShouldReturnApplicationUpdateSession()
    {
        // Arrange
        var registrationMaterial = new RegistrationMaterial
        {
            Id = 1,
            MaterialName = "Plastic",
            Status = ApplicationStatus.Granted,
            StatusUpdatedByName = "Test User",
            StatusUpdatedAt = DateTime.Now,
            RegistrationNumber = "ABC1234"
        };

        // Act
        var applicationUpdateSession = _mapper.Map<ApplicationUpdateSession>(registrationMaterial);

        // Assert
        applicationUpdateSession.Should().NotBeNull();
        applicationUpdateSession.RegistrationMaterialId.Should().Be(registrationMaterial.Id);
        applicationUpdateSession.RegistrationId.Should().Be(registrationMaterial.RegistrationId);
        applicationUpdateSession.MaterialName.Should().Be(registrationMaterial.MaterialName);
        applicationUpdateSession.Status.Should().Be(registrationMaterial.Status);
    }

    [TestMethod]
    public void Map_WhenCalledWithApplicationUpdateSession_ShouldReturnApplicationGrantedViewModel()
    {
        // Arrange
        var applicationUpdateSession = CreateApplicationUpdateSession();

        // Act
        var applicationGrantedViewModel = _mapper.Map<ApplicationGrantedViewModel>(applicationUpdateSession);

        // Assert
        applicationGrantedViewModel.Should().NotBeNull();
        applicationGrantedViewModel.Comments.Should().BeNull();
        applicationGrantedViewModel.MaterialName.Should().Be(applicationUpdateSession.MaterialName);
    }
    
    [TestMethod]
    public void Map_WhenCalledWithApplicationUpdateSession_ShouldReturnApplicationRefusedViewModel()
    {
        // Arrange
        var applicationUpdateSession = CreateApplicationUpdateSession();

        // Act
        var applicationRefusedViewModel = _mapper.Map<ApplicationRefusedViewModel>(applicationUpdateSession);

        // Assert
        applicationRefusedViewModel.Should().NotBeNull();
        applicationRefusedViewModel.Comments.Should().BeNull();
        applicationRefusedViewModel.MaterialName.Should().Be(applicationUpdateSession.MaterialName);
    }

    [TestMethod]
    public void Map_WhenCalledWithApplicationUpdateSession_ShouldReturnApplicationUpdateViewModel()
    {
        // Arrange
        var applicationUpdateSession = CreateApplicationUpdateSession();

        // Act
        var applicationUpdateViewModel = _mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

        // Assert
        applicationUpdateViewModel.Should().NotBeNull();
        applicationUpdateViewModel.Status.Should().Be(applicationUpdateSession.Status);
        applicationUpdateViewModel.MaterialName.Should().Be(applicationUpdateSession.MaterialName);
    }

    private static ApplicationUpdateSession CreateApplicationUpdateSession()
    {
        var applicationUpdateSession = new ApplicationUpdateSession
        {
            RegistrationMaterialId = 1,
            RegistrationId = 10,
            MaterialName = "Plastic",
            Status = ApplicationStatus.Granted
        };
        return applicationUpdateSession;
    }
}