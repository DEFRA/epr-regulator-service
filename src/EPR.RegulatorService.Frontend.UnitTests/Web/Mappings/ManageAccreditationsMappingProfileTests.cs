using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class ManageAccreditationsMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ManageAccreditationsMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void Map_Registration_To_ManageAccreditationsViewModel_ShouldMapCorrectly()
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            OrganisationName = "Test Org",
            SiteAddress = "123 Test Lane",
            SiteGridReference = "AB1234",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "EA",
            Tasks = [
                new RegistrationTask
                {
                    Id = Guid.NewGuid(),
                    TaskName = RegulatorTaskType.AssignOfficer,
                    Status = RegulatorTaskStatus.Completed
                }
            ],
            Materials = [
                new RegistrationMaterialSummary
                {
                    Id = Guid.NewGuid(),
                    MaterialName = "Plastic",
                    Accreditations = [
                        new Accreditation
                        {
                            Id = Guid.NewGuid(),
                            ApplicationReference = "APP-123",
                            Status = "Approved",
                            DeterminationDate = DateTime.Today,
                            AccreditationYear = 2025,
                            Tasks = [
                                new AccreditationTask
                                {
                                    Id = Guid.NewGuid(),
                                    TaskName = "Business plan",
                                    Status = "Approved",
                                    Year = 2025
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var viewModel = _mapper.Map<ManageAccreditationsViewModel>(registration);

        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(registration.Id);
            viewModel.OrganisationName.Should().Be(registration.OrganisationName);
            viewModel.SiteAddress.Should().Be(registration.SiteAddress);
            viewModel.SiteGridReference.Should().Be(registration.SiteGridReference);
            viewModel.ApplicationType.Should().Be("Reprocessor");
            viewModel.Regulator.Should().Be(registration.Regulator);
            viewModel.SiteLevelTasks.Should().HaveCount(1);
            viewModel.Materials.Should().HaveCount(1);
            viewModel.Materials.Single().Accreditation.ApplicationReference.Should().Be("APP-123");
        }
    }

    [TestMethod]
    public void Map_Accreditation_To_AccreditationDetailsViewModel_ShouldMapCorrectly()
    {
        var prnTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "PRN tonnage and authority to issue PRNs", // ✅ Matches mapping enum logic
            Status = "Not Started",
            Year = 2026
        };

        var businessPlanTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "Business plan", // ✅ Matches mapping enum logic
            Status = "Approved",
            Year = 2026
        };

        var samplingTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "Sampling and inspection plan", // ✅ Matches mapping enum logic
            Status = "Queried",
            Year = 2026
        };

        var accreditation = new Accreditation
        {
            Id = Guid.NewGuid(),
            ApplicationReference = "APP-456",
            Status = "Pending",
            DeterminationDate = DateTime.Today,
            AccreditationYear = 2026,
            Tasks = new List<AccreditationTask> { prnTask, businessPlanTask, samplingTask }
        };

        var viewModel = _mapper.Map<AccreditationDetailsViewModel>(accreditation);

        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(accreditation.Id);
            viewModel.ApplicationReference.Should().Be("APP-456");
            viewModel.Status.Should().Be("Pending");
            viewModel.DeterminationDate.Should().Be(accreditation.DeterminationDate);
            viewModel.AccreditationYear.Should().Be(2026);

            viewModel.PRNTonnageTask.Should().NotBeNull();
            viewModel.PRNTonnageTask!.StatusText.Should().Be("Not Started");
            viewModel.PRNTonnageTask.StatusCssClass.Should().Be("govuk-tag--grey");

            viewModel.BusinessPlanTask.Should().NotBeNull();
            viewModel.BusinessPlanTask!.StatusText.Should().Be("Approved");
            viewModel.BusinessPlanTask.StatusCssClass.Should().Be("govuk-tag--green");

            viewModel.SamplingAndInspectionPlanTask.Should().NotBeNull();
            viewModel.SamplingAndInspectionPlanTask!.StatusText.Should().Be("Queried");
            viewModel.SamplingAndInspectionPlanTask.StatusCssClass.Should().Be("govuk-tag--orange");
        }
    }


    [TestMethod]
    public void Map_RegistrationTask_To_AccreditationTaskViewModel_ShouldMapCorrectly()
    {
        // Arrange
        var registrationTask = new RegistrationTask
        {
            Id = Guid.NewGuid(),
            TaskName = RegulatorTaskType.AssignOfficer,
            Status = RegulatorTaskStatus.Completed
        };

        // Act
        var viewModel = _mapper.Map<AccreditationTaskViewModel>(registrationTask);

        // Assert
        using (new AssertionScope())
        {
            viewModel.TaskName.Should().Be(RegulatorTaskType.AssignOfficer);
            viewModel.StatusText.Should().Be("Officer Assigned");
            viewModel.StatusCssClass.Should().Be("govuk-tag--blue");
        }
    }
}
