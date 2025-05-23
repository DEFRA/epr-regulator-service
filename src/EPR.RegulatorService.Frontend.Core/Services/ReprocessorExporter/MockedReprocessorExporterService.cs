using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

/// <summary>
/// This service provides mock registration data for testing purposes.
/// It follows a simple convention:
/// - If the ID is even, the organisation is a Reprocessor.
/// - If the ID is odd, the organisation is an Exporter.
/// This logic is purely for testing and will be replaced with real database queries.
/// </summary>
[ExcludeFromCodeCoverage]
public class MockedReprocessorExporterService : IReprocessorExporterService
{
    private readonly List<Registration> _registrations = SeedRegistrations();

    public Task<Registration> GetRegistrationByIdAsync(int id)
    {
        if (id == 99999)
        {
            throw new NotFoundException("Mocked exception for testing purposes.");
        }

        // Determine if the registration is for an Exporter or Reprocessor
        // Even IDs return Reprocessor, Odd IDs return Exporter (mock convention)
        var organisationType = id % 2 == 0
            ? ApplicationOrganisationType.Reprocessor
            : ApplicationOrganisationType.Exporter;

        var registration = _registrations.FirstOrDefault(r => r.Id == id);

        if (registration == null)
        {
            registration = organisationType == ApplicationOrganisationType.Reprocessor
                ? CreateReprocessorRegistration(id)
                : CreateExporterRegistration(id);

            _registrations.Add(registration);
        }

        return Task.FromResult(registration);
    }

    public Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(int id)
    {
        if (id == 99999)
        {
            throw new NotFoundException("Mocked exception for testing purposes.");
        }

        var siteDetails = new SiteDetails
        {
            RegistrationId = id,
            SiteAddress = "16 Ruby St, London, E12 3SE",
            NationName = "England",
            GridReference = "SJ 854 662",
            LegalCorrespondenceAddress = "25 Ruby St, London, E12 3SE",
        };

        return Task.FromResult(siteDetails);
    }

    public Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(int registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials)
            .FirstOrDefault(rm => rm.Id == registrationMaterialId);

        if (registrationMaterial == null)
        {
            throw new NotFoundException("Registration material not found.");
        }

        return Task.FromResult(new RegistrationMaterialDetail
        {
            Id = registrationMaterial.Id,
            RegistrationId = registrationMaterial.RegistrationId,
            MaterialName = registrationMaterial.MaterialName,
            Status = registrationMaterial.Status,
        });
    }

    public Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(int registrationId) =>
        Task.FromResult(new RegistrationAuthorisedMaterials
        {
            RegistrationId = registrationId,
            OrganisationName = "MOCK Test Org",
            SiteAddress = "MOCK Test Address",
            MaterialsAuthorisation =
            [
                new MaterialsAuthorisedOnSite { IsMaterialRegistered = true, MaterialName = "Plastic" },
                new MaterialsAuthorisedOnSite
                {
                    IsMaterialRegistered = false,
                    MaterialName = "Steel",
                    Reason =
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce vulputate aliquet ornare. Vestibulum dolor nunc, tincidunt a diam nec, mattis venenatis sem"
                }
            ]
        });

    public Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials)
            .First(rm => rm.Id == registrationMaterialId);

        var registration = _registrations.Single(r => r.Id == registrationMaterial.RegistrationId);

        return Task.FromResult(new RegistrationMaterialPaymentFees
        {
            RegistrationId = registration.Id,
            OrganisationName = registration.OrganisationName,
            ApplicationType = registration.OrganisationType,
            SiteAddress = registration.SiteAddress,
            RegistrationMaterialId = registrationMaterial.Id,
            MaterialName = registrationMaterial.MaterialName,
            FeeAmount = 2921,
            ApplicationReferenceNumber = "ABC123456",
            SubmittedDate = DateTime.Now.AddDays(-7),
            Regulator = "GB-ENG"
        });
    }

    public Task MarkAsDulyMadeAsync(int registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);

        registrationMaterial.DeterminationDate = dulyMadeRequest.DeterminationDate;

        var task = registrationMaterial.Tasks.SingleOrDefault(t => t.TaskName == RegulatorTaskType.CheckRegistrationStatus);
        int? taskId;

        if (task == null)
        {
            taskId = (registrationMaterialId * 1000) + registrationMaterial.Tasks.Count;
        }
        else
        {
            taskId = task.Id;
            registrationMaterial.Tasks.Remove(task);
        }

        var newTask = new RegistrationTask
        {
            Id = taskId,
            TaskName = RegulatorTaskType.CheckRegistrationStatus,
            Status = RegulatorTaskStatus.Completed
        };

        registrationMaterial.Tasks.Add(newTask);

        return Task.CompletedTask;
    }

    public Task SubmitOfflinePaymentAsync(OfflinePaymentRequest offlinePayment) => Task.CompletedTask;

    public Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);
        var registration = _registrations.First(r => r.Id == registrationMaterial.RegistrationId);

        registration.Materials.Remove(registrationMaterial);

        string? registrationNumber = registrationMaterialOutcomeRequest.Status == ApplicationStatus.Granted ? "ABC123 4563" : null;
        string? statusUpdatedBy = registrationMaterialOutcomeRequest.Status != null ? "Test User" : null;
        DateTime? statusUpdatedAt = registrationMaterialOutcomeRequest.Status != null ? DateTime.Now : null;
        string? applicationNumber = null;

        var updatedRegistrationMaterial = CreateRegistrationMaterial(
            registrationMaterial.RegistrationId,
            registrationMaterial.MaterialName,
            registration.OrganisationType,
            applicationNumber,
            registrationMaterialOutcomeRequest.Status,
            statusUpdatedBy,
            statusUpdatedAt,
            registrationNumber);

        registration.Materials.Add(updatedRegistrationMaterial);

        return Task.CompletedTask;
    }

    public Task UpdateRegulatorRegistrationTaskStatusAsync(UpdateRegistrationTaskStatusRequest updateRegistrationTaskStatusRequest)
    {
        var registration = _registrations.Single(r => r.Id == updateRegistrationTaskStatusRequest.RegistrationId);
        var task = registration.Tasks.SingleOrDefault(t => t.TaskName.ToString() == updateRegistrationTaskStatusRequest.TaskName);
        int? taskId;

        if (task == null)
        {
            taskId = (updateRegistrationTaskStatusRequest.RegistrationId * 10) + registration.Tasks.Count;
        }
        else
        {
            taskId = task.Id;
            registration.Tasks.Remove(task);
        }

        var newTask = new RegistrationTask
        {
            Id = taskId,
            TaskName = Enum.Parse<RegulatorTaskType>(updateRegistrationTaskStatusRequest.TaskName),
            Status = Enum.Parse<RegulatorTaskStatus>(updateRegistrationTaskStatusRequest.Status)
        };

        registration.Tasks.Add(newTask);

        return Task.CompletedTask;
    }

    public Task UpdateRegulatorApplicationTaskStatusAsync(UpdateMaterialTaskStatusRequest updateMaterialTaskStatusRequest)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == updateMaterialTaskStatusRequest.RegistrationMaterialId);
        var task = registrationMaterial.Tasks.SingleOrDefault(t => t.TaskName.ToString() == updateMaterialTaskStatusRequest.TaskName);
        int? taskId;

        if (task == null)
        {
            taskId = (updateMaterialTaskStatusRequest.RegistrationMaterialId * 1000) + registrationMaterial.Tasks.Count;
        }
        else
        {
            taskId = task.Id;
            registrationMaterial.Tasks.Remove(task);
        }

        var newTask = new RegistrationTask
        {
            Id = taskId,
            TaskName = Enum.Parse<RegulatorTaskType>(updateMaterialTaskStatusRequest.TaskName),
            Status = Enum.Parse<RegulatorTaskStatus>(updateMaterialTaskStatusRequest.Status)
        };

        registrationMaterial.Tasks.Add(newTask);

        return Task.CompletedTask;
    }

    public Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(int registrationMaterialId) => throw new NotImplementedException();
    public Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(int registrationMaterialId) => throw new NotImplementedException();

    public Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        var registrationMaterialWasteLicence = new RegistrationMaterialWasteLicence
        {
            RegistrationMaterialId = registrationMaterialId,
            CapacityPeriod = "Per Year",
            CapacityTonne = 50000,
            LicenceNumbers = ["DFG34573453, ABC34573453, GHI34573453"],
            MaterialName = "Plastic",
            MaximumReprocessingCapacityTonne = 10000,
            MaximumReprocessingPeriod = "Per Month",
            PermitType = "Waste Exemption",
        };

        return Task.FromResult(registrationMaterialWasteLicence);
    }

    private static List<Registration> SeedRegistrations() =>
    [
        CreateExporterRegistration(1),
        CreateReprocessorRegistration(2)
    ];

    private static Registration CreateReprocessorRegistration(int registrationId)
    {
        const ApplicationOrganisationType organisationType = ApplicationOrganisationType.Reprocessor;

        return
            new Registration
            {
                Id = registrationId,
                OrganisationName = "MOCK Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = organisationType,
                Regulator = "Environment Agency (EA)",
                Tasks = CreateRegistrationTasks(registrationId, organisationType),
                Materials =
                [
                    CreateRegistrationMaterial(registrationId, "Plastic", organisationType, "222019EFGF"),
                    CreateRegistrationMaterial(registrationId, "Steel", organisationType, "333019EFGF"),
                    CreateRegistrationMaterial(registrationId, "Aluminium", organisationType, "444019EFGF"),
                ]
            };
    }

    private static Registration CreateExporterRegistration(int registrationId)
    {
        const ApplicationOrganisationType organisationType = ApplicationOrganisationType.Exporter;

        return new Registration
        {
            Id = registrationId,
            OrganisationName = "MOCK Blue Exports Ltd",
            SiteAddress = "N/A",
            OrganisationType = organisationType,
            Regulator = "Environment Agency (EA)",
            Tasks = CreateRegistrationTasks(registrationId, organisationType),
            Materials =
            [
                CreateRegistrationMaterial(registrationId, "Plastic", organisationType, "111019EFGF")
            ]
        };
    }

    private static List<RegistrationTask> CreateRegistrationTasks(int registrationId, ApplicationOrganisationType organisationType)
    {
        int taskId = registrationId * 100;

        if (organisationType == ApplicationOrganisationType.Reprocessor)
        {
            return
            [
                new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SiteAddressAndContactDetails },
                new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite }
            ];
        }

        return
        [
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.BusinessAddress },
            new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions }
        ];
    }

#pragma warning disable S107 - Ignoring the number of parameters in this method as it is required for testing purposes
    private static RegistrationMaterialSummary CreateRegistrationMaterial(
        int registrationId,
        string materialName,
        ApplicationOrganisationType organisationType,
        string applicationNumber,
        ApplicationStatus? status = null,
        string? statusUpdatedBy = null,
        DateTime? statusUpdatedAt = null,
        string? registrationNumber = null)
#pragma warning restore S107
    {
        int registrationMaterialId = registrationId * 10;

        return new RegistrationMaterialSummary
        {
            Id = registrationMaterialId,
            RegistrationId = registrationId,
            MaterialName = materialName,
            DeterminationDate = organisationType == ApplicationOrganisationType.Reprocessor ? DateTime.Now.AddDays(-7) : null,
            Status = status,
            StatusUpdatedBy = statusUpdatedBy,
            StatusUpdatedDate = statusUpdatedAt,
            RegistrationReferenceNumber = registrationNumber,
            ApplicationReferenceNumber = applicationNumber,
            Tasks = CreateMaterialTasks(registrationMaterialId, organisationType)
        };
    }

    private static List<RegistrationTask> CreateMaterialTasks(int registrationMaterialId, ApplicationOrganisationType organisationType)
    {
        int taskId = registrationMaterialId * 1000;

        if (organisationType == ApplicationOrganisationType.Reprocessor)
        {
            return
            [
                new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions },
                new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.CheckRegistrationStatus },
                new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs },
                new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
            ];
        }

        return
        [
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialDetailsAndContact },
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.CheckRegistrationStatus},
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails},
            new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
        ];
    }

    public async Task<HttpResponseMessage> DownloadSamplingInspectionFile(FileDownloadRequest request) => throw new NotImplementedException();

    public Task<Registration> GetRegistrationByIdWithAccreditationsAsync(Guid id, int? year = null)
    {
        var registration = GetMockedAccreditationRegistration(id);

        if (year.HasValue)
        {
            ApplySingleYearAccreditationFilter(registration, year.Value);
        }

        return Task.FromResult(registration);
    }

    private static Registration GetMockedAccreditationRegistration(Guid id)
    {
        var organisationType = ApplicationOrganisationType.Reprocessor; // Mocks always use Reprocessor

        var commonTasks = new List<AccreditationTask>
    {
        new AccreditationTask
        {
            IdGuid = Guid.Parse("c1d1f3d2-0c59-4c6e-8c8b-4e8eec2ec001"),
            TaskId = 1,
            TaskName = "PRN tonnage",
            Status = "Not Started Yet",
            Year = "2025"
        },
        new AccreditationTask
        {
            IdGuid = Guid.Parse("c1d1f3d2-0c59-4c6e-8c8b-4e8eec2ec002"),
            TaskId = 2,
            TaskName = "Business plan",
            Status = "Not Started Yet",
            Year = "2025"
        },
        new AccreditationTask
        {
            IdGuid = Guid.Parse("c1d1f3d2-0c59-4c6e-8c8b-4e8eec2ec003"),
            TaskId = 3,
            TaskName = "Sampling",
            Status = "Approved",
            Year = "2025"
        },
        new AccreditationTask
        {
            IdGuid = Guid.Parse("c1d1f3d2-0c59-4c6e-8c8b-4e8eec2ec004"),
            TaskId = 4,
            TaskName = "Determine accreditation",
            Status = "Not Started Yet",
            Year = "2025"
        }
    };

        return new Registration
        {
            IdGuid = id,
            OrganisationName = "Mock Green Ltd",
            SiteAddress = "23 Ruby St, London",
            OrganisationType = organisationType,
            Regulator = "EA",
            Materials = new List<RegistrationMaterialSummary>
        {
            new RegistrationMaterialSummary
            {
                IdGuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                MaterialName = "Plastic",
                Accreditations = new List<Accreditation>
                {
                    CreateAccreditation("aaaa1111-1111-1111-1111-111111111111", "MOCK-2025-PLASTIC", "Granted", new DateTime(2025, 6, 2, 0, 0, 0, DateTimeKind.Utc), 2025, commonTasks),
                    CreateAccreditation("aaaa2222-2222-2222-2222-222222222222", "MOCK-2026-PLASTIC", "Pending", new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2026, commonTasks),
                    CreateAccreditation("cccc0000-0000-0000-0000-000000000001", "MOCK-2027-PLASTIC-A", "Pending", new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Utc), 2027, commonTasks),
                    CreateAccreditation("cccc0000-0000-0000-0000-000000000002", "MOCK-2027-PLASTIC-B", "Pending", new DateTime(2027, 8, 19, 0, 0, 0, DateTimeKind.Utc), 2027, commonTasks)
                }
            },
            new RegistrationMaterialSummary
            {
                IdGuid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                MaterialName = "Steel",
                Accreditations = new List<Accreditation>
                {
                    CreateAccreditation("bbbb1111-1111-1111-1111-111111111111", "MOCK-2025-STEEL", "Granted", new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc), 2025, commonTasks),
                    CreateAccreditation("bbbb2222-2222-2222-2222-222222222222", "MOCK-2026-STEEL", "In Review", new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc), 2026, commonTasks)
                }
            }
        },
            Tasks = new List<RegistrationTask>
        {
            new RegistrationTask
            {
                IdGuid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                TaskName = RegulatorTaskType.AssignOfficer,
                Status = RegulatorTaskStatus.Completed
            }
        }
        };
    }

    private static Accreditation CreateAccreditation(string guid, string reference, string status, DateTime date, int year, List<AccreditationTask> tasks)
    {
        return new Accreditation
        {
            IdGuid = Guid.Parse(guid),
            ApplicationReference = reference,
            Status = status,
            DeterminationDate = date,
            AccreditationYear = year,
            Tasks = tasks
        };
    }

    private static void ApplySingleYearAccreditationFilter(Registration registration, int year)
    {
        foreach (var material in registration.Materials)
        {
            var matches = material.Accreditations
                .Where(a => a.AccreditationYear == year)
                .ToList();

            if (matches.Count == 0)
            {
                throw new InvalidOperationException($"No accreditation found for {material.MaterialName} in year {year}.");
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Multiple accreditations found for {material.MaterialName} in year {year}.");
            }

            material.Accreditations = matches;
        }
    }
}