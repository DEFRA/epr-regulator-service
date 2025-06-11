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
            Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            OrganisationName = "Test Organisation",
            SiteAddress = "123 Test Street",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Custom Regulator",
            Materials =
            [
                new RegistrationMaterialSummary
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    MaterialName = "Plastic",
                    Status = ApplicationStatus.Granted,
                    StatusUpdatedBy = "Test User",
                    StatusUpdatedDate = DateTime.Now,
                    RegistrationReferenceNumber = "ABC1234"
                }
            ],
            Tasks =
            [
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.BusinessAddress
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.SiteAddressAndContactDetails
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions
                }
            ]
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
            viewModel.BusinessAddressTask.Should().NotBeNull();
            viewModel.MaterialsAuthorisedOnSiteTask.Should().NotBeNull();
            viewModel.ExporterWasteLicensesTask.Should().NotBeNull();
            viewModel.SiteAddressTask.Should().NotBeNull();
            viewModel.Materials.Should().NotBeEmpty();
        }
    }

    [TestMethod]
    [DataRow(RegulatorTaskStatus.NotStarted,  "govuk-tag--grey")]
    [DataRow(RegulatorTaskStatus.Queried, "govuk-tag--orange")]
    [DataRow(RegulatorTaskStatus.Completed,  "govuk-tag--blue")]
    public void Map_WhenCalledWithRegistrationTask_ShouldReturnRegistrationTaskViewModelToCheckStatusCss(RegulatorTaskStatus status,
         string expectedCssClass)
    {
        // Arrange
        var registrationTask = new RegistrationTask { Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), Status = status, TaskName = RegulatorTaskType.SiteAddressAndContactDetails };

        // Act
        var viewModel = _mapper.Map<RegistrationTaskViewModel>(registrationTask);

        // Assert
        using (new AssertionScope())
        {
            viewModel.TaskName.Should().Be(registrationTask.TaskName);
            viewModel.StatusCssClass.Should().Be(expectedCssClass);
        }
    }

    [TestMethod]
    [DataRow(RegulatorTaskStatus.NotStarted, "Not started yet")]
    [DataRow(RegulatorTaskStatus.Queried, "Queried")]
    public void Map_WhenCalledWithNonCompletedRegistrationTask_ShouldReturnExpectedStatusText(RegulatorTaskStatus status, string expectedStatusText)
    {
        // Arrange
        var registrationTask = new RegistrationTask { Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), Status = status, TaskName = RegulatorTaskType.SamplingAndInspectionPlan };

        // Act
        var viewModel = _mapper.Map<RegistrationTaskViewModel>(registrationTask);

        // Assert
        using (new AssertionScope())
        {
            viewModel.TaskName.Should().Be(registrationTask.TaskName);
            viewModel.StatusText.Should().Be(expectedStatusText);
        }
    }

    [TestMethod]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.CheckRegistrationStatus, "Duly Made")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.SamplingAndInspectionPlan, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.AssignOfficer, "Officer Assigned")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.SiteAddressAndContactDetails, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.WasteLicensesPermitsAndExemptions, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.ReprocessingInputsAndOutputs, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.MaterialsAuthorisedOnSite, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.MaterialDetailsAndContact, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails, "Reviewed")]
    [DataRow(RegulatorTaskStatus.Completed, RegulatorTaskType.BusinessAddress, "Reviewed")]
    public void Map_WhenCalledWithCompletedRegistrationTask_ShouldReturnExpectedStatusText(RegulatorTaskStatus status, RegulatorTaskType taskName, string expectedStatusText)
    {
        // Arrange
        var registrationTask = new RegistrationTask { Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), Status = status, TaskName = taskName };

        // Act
        var viewModel = _mapper.Map<RegistrationTaskViewModel>(registrationTask);

        // Assert
        using (new AssertionScope())
        {
            viewModel.TaskName.Should().Be(registrationTask.TaskName);
            viewModel.StatusText.Should().Be(expectedStatusText);
        }
    }

    [TestMethod]
    [DataRow(null, "Not started yet", "govuk-tag--grey")]
    [DataRow(ApplicationStatus.Granted, "Granted", "govuk-tag--green")]
    [DataRow(ApplicationStatus.Refused, "Refused", "govuk-tag--red")]
    public void Map_WhenCalledWithRegistrationMaterial_ShouldReturnManageRegistrationMaterialViewModel(
        ApplicationStatus? applicationStatus, string expectedStatusText, string expectedCssClass)
    {
        // Arrange
        var registrationMaterial = new RegistrationMaterialSummary
        {
            Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            MaterialName = "Plastic",
            DeterminationDate = DateTime.Now.AddDays(-1),
            Status = applicationStatus,
            StatusUpdatedBy = "Test User",
            StatusUpdatedDate = DateTime.Now,
            RegistrationReferenceNumber = "ABC1234",
            Tasks =
            [
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.SamplingAndInspectionPlan
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.MaterialDetailsAndContact
                },
                new RegistrationTask
                {
                    Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
                    Status = RegulatorTaskStatus.NotStarted,
                    TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails
                }
            ]
        };

        // Act
        var viewModel = _mapper.Map<ManageRegistrationMaterialViewModel>(registrationMaterial);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Should().NotBeNull();
            viewModel.Id.Should().Be(registrationMaterial.Id);
            viewModel.MaterialName.Should().Be(registrationMaterial.MaterialName);
            viewModel.DeterminationDate.Should().Be(registrationMaterial.DeterminationDate);
            viewModel.Status.Should().Be(registrationMaterial.Status);
            viewModel.StatusUpdatedBy.Should().Be(registrationMaterial.StatusUpdatedBy);
            viewModel.StatusUpdatedDate.Should().Be(registrationMaterial.StatusUpdatedDate);
            viewModel.RegistrationReferenceNumber.Should().Be(registrationMaterial.RegistrationReferenceNumber);
            viewModel.StatusText.Should().Be(expectedStatusText);
            viewModel.StatusCssClass.Should().Be(expectedCssClass);
            viewModel.MaterialWasteLicensesTask.Should().NotBeNull();
            viewModel.InputsAndOutputsTask.Should().NotBeNull();
            viewModel.SamplingAndInspectionPlanTask.Should().NotBeNull();
            viewModel.MaterialDetailsTask.Should().NotBeNull();
            viewModel.OverseasReprocessorTask.Should().NotBeNull();
        }
    }
}
