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
            IdGuid = Guid.NewGuid(),
            OrganisationName = "Test Org",
            SiteAddress = "123 Test Lane",
            SiteGridReference = "AB1234",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "EA",
            Tasks = [
                new RegistrationTask
                {
                    IdGuid = Guid.NewGuid(),
                    TaskName = RegulatorTaskType.AssignOfficer,
                    Status = RegulatorTaskStatus.Completed
                }
            ],
            Materials = [
                new RegistrationMaterialSummary
                {
                    IdGuid = Guid.NewGuid(),
                    MaterialName = "Plastic",
                    Accreditations = [
                        new Accreditation
                        {
                            IdGuid = Guid.NewGuid(),
                            ApplicationReference = "APP-123",
                            Status = "Approved",
                            DeterminationDate = DateTime.Today,
                            AccreditationYear = 2025,
                            Tasks = [
                                new AccreditationTask
                                {
                                    IdGuid = Guid.NewGuid(),
                                    TaskId = 1,
                                    TaskName = "Business plan",
                                    Status = "Approved",
                                    Year = "2025"
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
            viewModel.Id.Should().Be(registration.IdGuid);
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
        var accreditation = new Accreditation
        {
            IdGuid = Guid.NewGuid(),
            ApplicationReference = "APP-456",
            Status = "Pending",
            DeterminationDate = DateTime.Today,
            AccreditationYear = 2026,
            Tasks = [
                new AccreditationTask
                {
                    IdGuid = Guid.NewGuid(),
                    TaskId = 2,
                    TaskName = "PRN tonnage",
                    Status = "Not Started",
                    Year = "2026"
                }
            ]
        };

        var viewModel = _mapper.Map<AccreditationDetailsViewModel>(accreditation);

        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(accreditation.IdGuid);
            viewModel.ApplicationReference.Should().Be("APP-456");
            viewModel.Status.Should().Be("Pending");
            viewModel.DeterminationDate.Should().Be(accreditation.DeterminationDate);
            viewModel.AccreditationYear.Should().Be(2026);
            viewModel.Tasks.Should().ContainSingle().Which.TaskName.Should().Be("PRN tonnage");
        }
    }

    [TestMethod]
    public void Map_RegistrationTask_To_AccreditationTaskViewModel_ShouldMapCorrectly()
    {
        var registrationTask = new RegistrationTask
        {
            IdGuid = Guid.NewGuid(),
            TaskName = RegulatorTaskType.AssignOfficer,
            Status = RegulatorTaskStatus.Completed
        };

        var viewModel = _mapper.Map<AccreditationTaskViewModel>(registrationTask);

        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(registrationTask.IdGuid?.ToString());
            viewModel.TaskId.Should().Be((int)registrationTask.TaskName);
            viewModel.TaskName.Should().Be(registrationTask.TaskName.ToString());
            viewModel.Status.Should().Be(registrationTask.Status.ToString());
            viewModel.Year.Should().BeNull();
        }
    }
}
