using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
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
    public void Map_WhenCalledWithRegistration_ShouldReturnManageRegistrationsViewModel()
    {
        // Arrange
        var registrationDto = new Registration
        {
            Id = 1,
            OrganisationName = "Test Organisation",
            SiteAddress = "123 Test Street",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Custom Regulator",
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

    [TestMethod]
    [DataRow(RegulatorTaskStatus.NotStarted, "Not started yet", "govuk-tag--grey")]
    [DataRow(RegulatorTaskStatus.Completed, "Reviewed", "govuk-tag--default")]
    [DataRow(RegulatorTaskStatus.Queried, "Queried", "govuk-tag--grey")]
    public void Map_WhenCalledWithRegistrationTask_ShouldReturnRegistrationTaskViewModel(RegulatorTaskStatus status, string expectedStatusText, string expectedCssClass)
    {
        // Arrange
        var registrationTask = new RegistrationTask
        {
            Id = 1, Status = status
        };

        // Act
        var viewModel = _mapper.Map<RegistrationTaskViewModel>(registrationTask);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(registrationTask.Id);
            viewModel.StatusText.Should().Be(expectedStatusText);
            viewModel.StatusCssClass.Should().Be(expectedCssClass);
        }
    }

    [TestMethod]
    [DataRow(null, "Not started yet", "govuk-tag--grey")]
    [DataRow(ApplicationStatus.Granted, "Granted", "govuk-tag--green")]
    [DataRow(ApplicationStatus.Refused, "Refused", "govuk-tag--red")]
    public void Map_WhenCalledWithRegistrationMaterial_ShouldReturnManageRegistrationMaterialViewModel(ApplicationStatus? applicationStatus, string expectedStatusText, string expectedCssClass)
    {
        // Arrange
        var registrationMaterial = new RegistrationMaterial
        {
            Id = 1,
            MaterialName = "Plastic",
            DeterminationDate = DateTime.Now.AddDays(-1),
            Status = applicationStatus,
            StatusUpdatedByName = "Test User",
            StatusUpdatedAt = DateTime.Now,
            RegistrationNumber = "ABC1234"
        };

        // Act
        var viewModel = _mapper.Map<ManageRegistrationMaterialViewModel>(registrationMaterial);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.Id.Should().Be(registrationMaterial.Id);
            viewModel.MaterialName.Should().Be(registrationMaterial.MaterialName);
            viewModel.Status.Should().Be(registrationMaterial.Status);
            viewModel.StatusUpdatedByName.Should().Be(registrationMaterial.StatusUpdatedByName);
            viewModel.StatusUpdatedAt.Should().Be(registrationMaterial.StatusUpdatedAt);
            viewModel.RegistrationNumber.Should().Be(registrationMaterial.RegistrationNumber);
            viewModel.StatusText.Should().Be(expectedStatusText);
            viewModel.StatusCssClass.Should().Be(expectedCssClass);
        }
    }
}
