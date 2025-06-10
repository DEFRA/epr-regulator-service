using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        // Arrange
        var prnTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "PRNTonnage", // Matches mapping enum logic
            Status = "Not Started",
            Year = 2026
        };

        var businessPlanTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "BusinessPlan", // Matches mapping enum logic
            Status = "Approved",
            Year = 2026
        };

        var samplingTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "SamplingAndInspectionPlan", // Matches mapping enum logic
            Status = "Queried",
            Year = 2026
        };

        var dulyMadeTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "DulyMade", // Matches mapping enum logic
            Status = "Completed",
            Year = 2026
        };

        var overseasReprocessing = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "OverseasReprocessingSites", // Matches mapping enum logic
            Status = "Completed",
            Year = 2026
        };

        var evidenceOfBroadlyEquivalentStandards = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskName = "EvidenceOfBroadlyEquivalentStandards", // Matches mapping enum logic
            Status = "Queried",
            Year = 2026
        };

        var accreditation = new Accreditation
        {
            Id = Guid.NewGuid(),
            ApplicationReference = "APP-456",
            Status = ApplicationStatus.Granted.ToString(), // Matching enum string
            DeterminationDate = DateTime.Today,
            AccreditationYear = 2026,
            Tasks = new List<AccreditationTask> { prnTask, businessPlanTask, samplingTask, dulyMadeTask, overseasReprocessing, evidenceOfBroadlyEquivalentStandards }
        };

        // Act
        var viewModel = _mapper.Map<AccreditationDetailsViewModel>(accreditation);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(accreditation.Id);
            viewModel.ApplicationReference.Should().Be("APP-456");
            viewModel.Status.Should().Be(ApplicationStatus.Granted);
            viewModel.DeterminationDate.Should().Be(accreditation.DeterminationDate);
            viewModel.AccreditationYear.Should().Be(2026);

            viewModel.PRNTonnageTask.Should().NotBeNull();
            viewModel.PRNTonnageTask!.StatusText.Should().Be("Not started yet");
            viewModel.PRNTonnageTask.StatusCssClass.Should().Be("govuk-tag--grey");

            viewModel.BusinessPlanTask.Should().NotBeNull();
            viewModel.BusinessPlanTask!.StatusText.Should().Be("Approved");
            viewModel.BusinessPlanTask.StatusCssClass.Should().Be("govuk-tag--green");

            viewModel.SamplingAndInspectionPlanTask.Should().NotBeNull();
            viewModel.SamplingAndInspectionPlanTask!.StatusText.Should().Be("Queried");
            viewModel.SamplingAndInspectionPlanTask.StatusCssClass.Should().Be("govuk-tag--orange");

            viewModel.CheckAccreditationStatusTask.Should().NotBeNull();
            viewModel.CheckAccreditationStatusTask!.StatusText.Should().Be("Duly Made");
            viewModel.CheckAccreditationStatusTask.StatusCssClass.Should().Be("govuk-tag--blue");

            viewModel.OverseasReprocessingSitesTask.Should().NotBeNull();
            viewModel.OverseasReprocessingSitesTask!.StatusText.Should().Be("Overseas Reprocessing Sites");
            viewModel.OverseasReprocessingSitesTask.StatusCssClass.Should().Be("govuk-tag--blue");

            viewModel.EvidenceOfBroadlyEquivalentStandardsTask.Should().NotBeNull();
            viewModel.EvidenceOfBroadlyEquivalentStandardsTask!.StatusText.Should().Be("Queried");
            viewModel.EvidenceOfBroadlyEquivalentStandardsTask.StatusCssClass.Should().Be("govuk-tag--orange");

            viewModel.ShouldDisplay.Should().BeTrue();
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

    [TestMethod]
    public void Map_RegistrationMaterialSummary_To_AccreditedMaterialViewModel_MapsStatusFieldsCorrectly()
    {
        // Arrange
        var material = new RegistrationMaterialSummary
        {
            Id = Guid.NewGuid(),
            MaterialName = "Aluminium",
            Status = ApplicationStatus.Refused
        };

        // Act
        var viewModel = _mapper.Map<AccreditedMaterialViewModel>(material);

        // Assert
        using (new AssertionScope())
        {
            viewModel.Id.Should().Be(material.Id);
            viewModel.MaterialName.Should().Be("Aluminium");

            // This is the raw enum
            viewModel.RegistrationStatusRaw.Should().Be(ApplicationStatus.Refused);

            // This is the mapped status text and tag
            viewModel.RegistrationStatusTask.Should().NotBeNull();
            viewModel.RegistrationStatusTask.StatusText.Should().Be("Refused");
            viewModel.RegistrationStatusTask.StatusCssClass.Should().Be("govuk-tag--red");

            viewModel.ShouldDisplay.Should().BeFalse();
        }
    }

    [TestMethod]
    public void Map_InvalidStatusString_To_Enum_ShouldFallbackToStarted()
    {
        var profile = new ManageAccreditationsMappingProfile();
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapStatusStringToEnum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (ApplicationStatus)method.Invoke(null, new object[] { "UNKNOWN_STATUS" });

        result.Should().Be(ApplicationStatus.Started);
    }

    [TestMethod]
    public void MapTaskStatusText_WithUnknownStringStatus_ShouldReturnDefault()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                       null, new[] { typeof(string) }, null);

        method.Should().NotBeNull("the method should be found with correct binding flags");

        var result = (string)method!.Invoke(null, new object[] { "UNKNOWN" });

        result.Should().Be("Not Started Yet");
    }

    [TestMethod]
    public void MapTaskStatusText_WithUnknownStatusAndTask_ShouldReturnNotStartedYet()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null,
                       new[] { typeof(string), typeof(RegulatorTaskType) }, null);

        method.Should().NotBeNull("the method should be found with correct binding flags");

        var result = (string)method!.Invoke(null, new object[] { "UNKNOWN", RegulatorTaskType.AssignOfficer });

        result.Should().Be("Not started yet");
    }

    [TestMethod]
    public void MapTaskStatusCssClass_WithUnknownStatus_ShouldReturnGrey()
    {
        var profile = new ManageAccreditationsMappingProfile();
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusCssClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (string)method.Invoke(null, new object[] { "UNKNOWN" });

        result.Should().Be("govuk-tag--grey");
    }

    [TestMethod]
    public void MapApplicationStatus_NullStatus_ShouldReturnDefaults()
    {
        var profile = new ManageAccreditationsMappingProfile();
        var mapText = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapApplicationStatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var mapClass = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapApplicationStatusCssClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var text = (string)mapText.Invoke(null, new object[] { null });
        var cssClass = (string)mapClass.Invoke(null, new object[] { null });

        text.Should().Be("Not started yet");
        cssClass.Should().Be("govuk-tag--grey");
    }

    [TestMethod]
    public void MapTaskStatusText_UnknownStatus_ShouldReturnNotStarted()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusText", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string) }, null);

        method.Should().NotBeNull();

        var result = (string)method!.Invoke(null, new object[] { "foobar" });

        result.Should().Be("Not Started Yet");
    }

    [TestMethod]
    public void MapTaskStatusText_CompletedStatus_UnknownTask_ShouldReturnReviewed()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusText", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string), typeof(RegulatorTaskType) }, null);

        method.Should().NotBeNull();

        var result = (string)method!.Invoke(null, new object[] { "completed", (RegulatorTaskType)999 });

        result.Should().Be("Reviewed");
    }

    [TestMethod]
    public void MapApplicationStatusText_UnknownStatus_ShouldReturnNotStartedYet()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapApplicationStatusText", BindingFlags.NonPublic | BindingFlags.Static);

        var result = (string)method!.Invoke(null, new object[] { (ApplicationStatus?)123 });

        result.Should().Be("Not started yet");
    }

    [TestMethod]
    public void MapApplicationStatusCssClass_UnknownStatus_ShouldReturnGrey()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapApplicationStatusCssClass", BindingFlags.NonPublic | BindingFlags.Static);

        var result = (string)method!.Invoke(null, new object[] { (ApplicationStatus?)123 });

        result.Should().Be("govuk-tag--grey");
    }

    [TestMethod]
    public void MapTaskStatusText_KnownStatuses_ShouldReturnExpectedResults()
    {
        var method = typeof(ManageAccreditationsMappingProfile)
            .GetMethod("MapTaskStatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new[] { typeof(string) }, null);

        method.Should().NotBeNull("the method should be found with correct binding flags");

        var testCases = new Dictionary<string, string>
        {
            { "not started", "Not Started yet" },
            { "not started yet", "Not started yet" },
            { "approved", "Approved" },
            { "queried", "Queried" },
            { "completed", "Completed" },
            { "duly made", "Duly Made" },
            { "dulymade", "Duly Made" }
        };

        foreach (var testCase in testCases)
        {
            var result = (string)method!.Invoke(null, new object[] { testCase.Key });

            result.Should().Be(testCase.Value, $"because '{testCase.Key}' should map to '{testCase.Value}'");
        }
    }
}
