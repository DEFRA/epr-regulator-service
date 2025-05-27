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

    public Task<Registration> GetRegistrationByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new NotFoundException("Mocked exception for testing purposes.");
        }

        var registration = _registrations.FirstOrDefault(r => r.Id == id);

        return Task.FromResult(registration);
    }

    public Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(Guid id)
    {
        if (id == Guid.Empty)
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

    public Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(Guid registrationMaterialId)
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

    public Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(Guid registrationId) =>
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

    public Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(Guid registrationMaterialId)
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

    public Task MarkAsDulyMadeAsync(Guid registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);

        registrationMaterial.DeterminationDate = dulyMadeRequest.DeterminationDate;

        var task = registrationMaterial.Tasks.SingleOrDefault(t => t.TaskName == RegulatorTaskType.CheckRegistrationStatus);
        Guid? taskId;

        if (task == null)
        {
            taskId = Guid.NewGuid();
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

    public Task UpdateRegistrationMaterialOutcomeAsync(Guid registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest)
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
        Guid? taskId;

        if (task == null)
        {
            taskId = Guid.NewGuid();
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
        Guid? taskId;

        if (task == null)
        {
            taskId = Guid.NewGuid();
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

    public Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(Guid registrationMaterialId) => throw new NotImplementedException();
    public Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(Guid registrationMaterialId) => throw new NotImplementedException();

    public Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(Guid registrationMaterialId)
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
        CreateExporterRegistration(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")),
        CreateReprocessorRegistration(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"))
    ];

    private static Registration CreateReprocessorRegistration(Guid registrationId)
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

    private static Registration CreateExporterRegistration(Guid registrationId)
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

    private static List<RegistrationTask> CreateRegistrationTasks(Guid registrationId, ApplicationOrganisationType organisationType)
    {
        if (organisationType == ApplicationOrganisationType.Reprocessor)
        {
            return
            [
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SiteAddressAndContactDetails },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite }
            ];
        }

        return
        [
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.BusinessAddress },
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions }
        ];
    }

#pragma warning disable S107 - Ignoring the number of parameters in this method as it is required for testing purposes
    private static RegistrationMaterialSummary CreateRegistrationMaterial(
        Guid registrationId,
        string materialName,
        ApplicationOrganisationType organisationType,
        string applicationNumber,
        ApplicationStatus? status = null,
        string? statusUpdatedBy = null,
        DateTime? statusUpdatedAt = null,
        string? registrationNumber = null)
#pragma warning restore S107
    {
        var registrationMaterialId = Guid.NewGuid();

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
            Tasks = CreateMaterialTasks(organisationType)
        };
    }

    private static List<RegistrationTask> CreateMaterialTasks(ApplicationOrganisationType organisationType)
    {
        if (organisationType == ApplicationOrganisationType.Reprocessor)
        {
            return
            [
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.CheckRegistrationStatus },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
            ];
        }

        return
        [
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialDetailsAndContact },
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.CheckRegistrationStatus},
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails},
            new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
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
        var organisationType = ApplicationOrganisationType.Reprocessor;

        return new Registration
        {
            Id = id,
            OrganisationName = "Mock Green Ltd",
            SiteAddress = "23 Ruby St, London",
            OrganisationType = organisationType,
            Regulator = "EA",
            Materials = new List<RegistrationMaterialSummary>
        {
            new RegistrationMaterialSummary
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                MaterialName = "Plastic",
                Status = ApplicationStatus.Granted,
                Accreditations = new List<Accreditation>
                {
                    CreateAccreditation("aaaa1111-1111-1111-1111-111111111111", "MOCK-2025-PLASTIC", "Granted", new DateTime(2025, 6, 2), 2025)
                }
            },
            new RegistrationMaterialSummary
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                MaterialName = "Steel",
                Status = ApplicationStatus.Refused,
                Accreditations = new List<Accreditation>
                {
                    CreateAccreditation("bbbb1111-1111-1111-1111-111111111111", "MOCK-2025-STEEL", "Refused", new DateTime(2025, 7, 15), 2025)
                }
            },
            new RegistrationMaterialSummary
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                MaterialName = "Glass",
                Status = null, // Tests fallback logic for "Not started yet"
                Accreditations = new List<Accreditation>()
            }
        },
            Tasks = new List<RegistrationTask>
        {
            new RegistrationTask
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                TaskName = RegulatorTaskType.AssignOfficer,
                Status = RegulatorTaskStatus.Completed
            }
        }
        };
    }


    private static Accreditation CreateAccreditation(string guid, string reference, string status, DateTime date, int year)
    {
        return new Accreditation
        {
            Id = Guid.Parse(guid),
            ApplicationReference = reference,
            Status = status,
            DeterminationDate = date,
            AccreditationYear = year,
            Tasks = new List<AccreditationTask>
            {
                new AccreditationTask
                {
                    Id = Guid.NewGuid(),
                    TaskId = 1,
                    TaskName = "PRN tonnage and authority to issue PRNs",
                    Status = "Not Started Yet",
                    Year = year
                },
                new AccreditationTask
                {
                    Id = Guid.NewGuid(),
                    TaskId = 2,
                    TaskName = "Business plan",
                    Status = "Not Started Yet",
                    Year = year
                },
                new AccreditationTask
                {
                    Id = Guid.NewGuid(),
                    TaskId = 3,
                    TaskName = "Sampling and inspection plan",
                    Status = "Approved",
                    Year = year
                }
            }
        };
    }


    private static void ApplySingleYearAccreditationFilter(Registration registration, int year)
    {
        bool hasAtLeastOneAccreditation = false;

        foreach (var material in registration.Materials)
        {
            var matchingAccreditations = material.Accreditations
                .Where(a => a.AccreditationYear == year)
                .ToList();

            if (matchingAccreditations.Count > 1)
            {
                throw new InvalidOperationException(
                    $"More than one accreditation found for MaterialId {material.Id} in year {year}.");
            }

            if (matchingAccreditations.Count == 1)
            {
                hasAtLeastOneAccreditation = true;
                material.Accreditations = matchingAccreditations;
            }
            else
            {
                material.Accreditations = new List<Accreditation>(); // Clean out non-matching years
            }
        }

        if (!hasAtLeastOneAccreditation)
        {
            throw new InvalidOperationException(
                $"No accreditations found for any materials in year {year}.");
        }
    }

}