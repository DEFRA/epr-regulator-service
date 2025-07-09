using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
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
    private Registration _registrationAccreditations = null;

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

        var registration = _registrations.Single(r => r.Id == id);
        var task = registration.Tasks.SingleOrDefault(t => t.TaskName == RegulatorTaskType.SiteAddressAndContactDetails);

        var siteDetails = new SiteDetails
        {
            RegistrationId = id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            NationName = "England",
            GridReference = "SJ 854 662",
            LegalCorrespondenceAddress = "25 Ruby St, London, E12 3SE",
            RegulatorRegistrationTaskStatusId = task?.Id,
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted
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

    public Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(Guid registrationId)
    {
        var task = _registrations.Single(r => r.Id == registrationId).Tasks.SingleOrDefault(t => t.TaskName == RegulatorTaskType.MaterialsAuthorisedOnSite);

        return Task.FromResult(new RegistrationAuthorisedMaterials
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
            ],
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted
        });

    }

    public Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials)
            .First(rm => rm.Id == registrationMaterialId);

        var registration = _registrations.Single(r => r.Id == registrationMaterial.RegistrationId);

        var task = registrationMaterial.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.CheckRegistrationStatus);

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
            Regulator = "GB-ENG",
            PaymentMethod = PaymentMethodType.BankTransfer,
            PaymentDate = DateTime.Now.AddDays(-7),
            DulyMadeDate = DateTime.Now.AddDays(-5),
            DeterminationDate = DateTime.Now.AddDays(+16),
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted,
            RegulatorApplicationTaskStatusId = task?.Id
        });
    }

    public Task<WasteCarrierDetails> GetWasteCarrierDetailsByRegistrationIdAsync(Guid registrationId)
    {
        var registration = _registrations.Single(r => r.Id == registrationId);

        var task = registration.Tasks.SingleOrDefault(t => t.TaskName == RegulatorTaskType.WasteCarrierBrokerDealerNumber);

        return Task.FromResult(new WasteCarrierDetails
        {
            RegistrationId = registration.Id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            WasteCarrierBrokerDealerNumber = "WCB123456789",
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted,
            RegulatorRegistrationTaskStatusId = task?.Id
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

    public Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);
        var registration = _registrations.Single(r => r.Id == registrationMaterial.RegistrationId);
        var task = registrationMaterial.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.ReprocessingInputsAndOutputs);
        
        var registrationMaterialReprocessingIO = new RegistrationMaterialReprocessingIO
        {
            OrganisationName = registration.OrganisationName,
            RegistrationId = registration.Id,
            SiteAddress = registration.SiteAddress!,
            RegistrationMaterialId = registrationMaterialId,
            MaterialName = "Plastic",
            SourcesOfPackagingWaste = "Shed",
            PlantEquipmentUsed = "shredder",
            UKPackagingWasteTonne = 6.00M,
            NonUKPackagingWasteTonne = 2.00M,
            NotPackingWasteTonne = 3.00M,
            SenttoOtherSiteTonne = 5.00M,
            ContaminantsTonne = 1.00M,
            ProcessLossTonne = 4.00M,
            TotalInputs = 7.00M,
            TotalOutputs = 8.00M,
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted
        };

        return Task.FromResult(registrationMaterialReprocessingIO);
    }

    public Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);
        var registration = _registrations.Single(r => r.Id == registrationMaterial.RegistrationId);
        var task = registrationMaterial.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.SamplingAndInspectionPlan);

        var registrationMaterialSamplingPlan = new RegistrationMaterialSamplingPlan
        {
            RegistrationId = registration.Id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            RegistrationMaterialId = registrationMaterialId,
            MaterialName = "Plastic",
            Files =
            [
                new RegistrationMaterialSamplingPlanFile
                {
                    Filename = "File0002-01-0.pdf",
                    FileUploadType = "PDF",
                    FileUploadStatus = "Completed",
                    FileId = "123",
                    UpdatedBy = "5d780e2d-5b43-4a45-92ac-7e2889582083",
                    DateUploaded = DateTime.UtcNow
                }

            ],
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted
        };
        return Task.FromResult(registrationMaterialSamplingPlan);
    }

    public Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        var registrationMaterial =
            _registrations.SelectMany(r => r.Materials).First(rm => rm.Id == registrationMaterialId);
        var registration = _registrations.Single(r => r.Id == registrationMaterial.RegistrationId);
        var task = registrationMaterial.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.WasteLicensesPermitsAndExemptions);

        var registrationMaterialWasteLicence = new RegistrationMaterialWasteLicence
        {
            RegistrationId = registration.Id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            RegistrationMaterialId = registrationMaterialId,
            CapacityPeriod = "Per Year",
            CapacityTonne = 50000,
            LicenceNumbers = ["DFG34573453, ABC34573453, GHI34573453"],
            MaterialName = "Plastic",
            MaximumReprocessingCapacityTonne = 10000,
            MaximumReprocessingPeriod = "Per Month",
            PermitType = "Waste Exemption",
            TaskStatus = task?.Status ?? RegulatorTaskStatus.NotStarted
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
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.WasteCarrierBrokerDealerNumber }
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
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan },
                new RegistrationTask { Id = Guid.NewGuid(), Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.DulyMade }
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
    
    public Task<AccreditationSamplingPlan> GetSamplingPlanByAccreditationIdAsync(Guid accreditationId)
    {
        return Task.FromResult(new AccreditationSamplingPlan
        {
            MaterialName = "Plastic",
            Files =
            [
                new AccreditationSamplingPlanFile
                {
                    Filename = "File0002-01-0.pdf",
                    FileUploadType = "PDF",
                    FileUploadStatus = "Completed",
                    FileId = "123",
                    UpdatedBy = "5d780e2d-5b43-4a45-92ac-7e2889582083",
                    DateUploaded = DateTime.UtcNow
                }

            ]
        });
    }

    public async Task<HttpResponseMessage> DownloadAccreditationSamplingInspectionFile(FileDownloadRequest request) => throw new NotImplementedException();

    public Task<Registration> GetRegistrationByIdWithAccreditationsAsync(Guid id, int? year = null)
    {
        _registrationAccreditations ??= GetMockedAccreditationRegistration(id);

        if (_registrationAccreditations == null)
        {
            throw new KeyNotFoundException($"No mock registration found for id {id}");
        }

        if (year.HasValue)
        {
            ApplySingleYearAccreditationFilter(_registrationAccreditations, year.Value);
        }

        return Task.FromResult(_registrationAccreditations);
    }

    private static Registration GetMockedAccreditationRegistration(Guid id)
    {
        var registrations = GetAllMockedRegistrations();
        return registrations.FirstOrDefault(r => r.Id == id);
    }

    private static List<Registration> GetAllMockedRegistrations()
    {
        return new List<Registration>
        {
            // Happy path: Granted + one accreditation (Granted)
            CreateMockRegistration(
                "839544fd-9b08-4823-9277-5615072a6803", "Mock Org 1", "Plastic",
                "49c5d1ec-8934-47d6-8cd7-4f465b7c0ad6", ApplicationStatus.Granted,
                new[] { ("dda7cd75-5fd3-44cb-accc-e4e9323b2af3", "Granted", 2025) }),

            // Material Withdrawn (should not show)
            CreateMockRegistration(
                "11dec0d4-b0db-44a6-84f3-3de06e46262c", "Mock Org 2", "Steel",
                "9f253f8b-60e5-44f8-953a-9b35300d1874", ApplicationStatus.Withdrawn,
                new[] { ("cea32558-d0ec-4efb-bb0d-7af2e425dd0c", "Granted", 2025) }),

            // Accreditation ReadyToSubmit (should not show)
            CreateMockRegistration(
                "12345678-aaaa-bbbb-cccc-111111111111", "Mock Org 3", "Wood",
                "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", ApplicationStatus.Granted,
                new[] { ("eeeeaaaa-1111-2222-3333-444444444444", "ReadyToSubmit", 2025) }),

            // No accreditation for that year (e.g. only 2024)
            CreateMockRegistration(
                "23456789-bbbb-cccc-dddd-222222222222", "Mock Org 4", "Glass",
                "bbbbbbbb-cccc-dddd-eeee-ffffffffffff", ApplicationStatus.Granted,
                new[] { ("ffffbbbb-5555-6666-7777-888888888888", "Granted", 2024) }),

            // Throws: More than one accreditation for same year
            CreateMockRegistration(
                "34567890-cccc-dddd-eeee-333333333333", "Mock Org 5", "Textiles",
                "cccccccc-dddd-eeee-ffff-000000000000", ApplicationStatus.Granted,
                new[]
                {
                    ("11112222-aaaa-bbbb-cccc-999999999999", "Granted", 2025),
                    ("22223333-bbbb-cccc-dddd-aaaaaaaaaaaa", "Granted", 2025)
                }),

            // Valid material with two years of accreditations
            CreateMockRegistration(
                "45678901-dddd-eeee-ffff-444444444444", "Mock Org 6", "Paper",
                "dddddddd-eeee-ffff-1111-222222222222", ApplicationStatus.Granted,
                new[]
                {
                    ("99990000-aaaa-bbbb-cccc-dddddddddddd", "Granted", 2025),
                    ("88887777-bbbb-cccc-dddd-eeeeeeeeeeee", "Granted", 2026)
                })
        };
    }

    private static Registration CreateMockRegistration(
    string registrationId,
    string orgName,
    string material,
    string materialId,
    ApplicationStatus materialStatus,
    (string id, string status, int year)[] accreditations)
    {
        return new Registration
        {
            Id = Guid.Parse(registrationId),
            OrganisationName = orgName,
            SiteAddress = "23 Ruby St, London",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "EA",
            Materials = new List<RegistrationMaterialSummary>
        {
            new RegistrationMaterialSummary
            {
                Id = Guid.Parse(materialId),
                MaterialName = material,
                Status = materialStatus,
                Accreditations = accreditations.Select(x =>
                    CreateAccreditation(
                        guid: x.id,
                        reference: $"MOCK-{x.year}-{material.ToUpper()}",
                        status: x.status,
                        date: new DateTime(x.year, 6, 1),
                        year: x.year)).ToList()
            }
        },
            Tasks = new List<RegistrationTask>
        {
            new RegistrationTask
            {
                Id = Guid.NewGuid(),
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

    public Task AddRegistrationQueryNoteAsync(Guid? regulatorRegistrationTaskStatusId, AddNoteRequest addNoteRequest) => Task.CompletedTask;

    public Task<AccreditationMaterialPaymentFees> GetPaymentFeesByAccreditationIdAsync(Guid id)
    {
        var mockPaymentFees = new AccreditationMaterialPaymentFees
        {
            AccreditationId = id,
            OrganisationName = "Mock Green Ltd",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            SiteAddress = "23 Ruby Street, London, E12 3SE",
            ApplicationReferenceNumber = "MOCK-REF-2025",
            MaterialName = "Plastic",
            SubmittedDate = new DateTime(2025, 5, 15),
            FeeAmount = 2921.00m,
            Regulator = "EA",
            PrnTonnage = PrnTonnageType.Upto5000Tonnes,
        };

        return Task.FromResult(mockPaymentFees);
    }

    public Task SubmitAccreditationOfflinePaymentAsync(AccreditationOfflinePaymentRequest offlinePayment) => Task.CompletedTask;

    public Task MarkAccreditationAsDulyMadeAsync(Guid accreditationId, AccreditationMarkAsDulyMadeRequest dulyMadeRequest) => Task.CompletedTask;

    public async Task UpdateRegulatorAccreditationTaskStatusAsync(UpdateAccreditationTaskStatusRequest updateAccreditationTaskStatusRequest)
    {
        _registrationAccreditations ??= await GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("839544fd-9b08-4823-9277-5615072a6803"), 2025);
        var accreditation = _registrationAccreditations.Materials
            .SelectMany(m => m.Accreditations)
            .FirstOrDefault(a => a.Id == updateAccreditationTaskStatusRequest.AccreditationId);

        var newTask = new AccreditationTask
        {
            Id = Guid.NewGuid(),
            TaskId = 4,
            TaskName = "Check accreditation status",
            Status = updateAccreditationTaskStatusRequest.Status,
            Year = 2025
        };

        accreditation?.Tasks.Add(newTask);
    }

    private static void ApplySingleYearAccreditationFilter(Registration registration, int year)
    {
        bool hasAtLeastOneAccreditation = false;

        foreach (var material in registration.Materials)
        {
            var matchingAccreditations = material.Accreditations?
                .Where(a => a.AccreditationYear == year)
                .ToList() ?? new List<Accreditation>();

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
                material.Accreditations = new List<Accreditation>();
            }
        }

        if (!hasAtLeastOneAccreditation)
        {
            throw new InvalidOperationException(
                $"No accreditations found for any materials in year {year}.");
        }
    }

    public Task AddMaterialQueryNoteAsync(Guid? regulatorApplicationTaskStatusId, AddNoteRequest addNoteRequest) => Task.CompletedTask;

    public Task AddRegistrationQueryNoteAsync(Guid regulatorRegistrationTaskStatusId, AddNoteRequest addNoteRequest) => Task.CompletedTask;
    public Task<AccreditationBusinessPlanDto> GetAccreditionBusinessPlanByIdAsync(Guid id) => CreateAccreditationBusinessPlan(id);   

    private static async Task<AccreditationBusinessPlanDto> CreateAccreditationBusinessPlan(Guid id)
    {
        var queryNotes = new List<QueryNoteResponseDto>();

        var accreditationId = id;

        var queryNote1 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "First Note"
        };

        var queryNote2 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        var queryNote3 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        queryNotes.Add(queryNote1);
        queryNotes.Add(queryNote2);
        queryNotes.Add(queryNote3);

        var accreditationBusinessPlanDto = new AccreditationBusinessPlanDto
        {
            AccreditationId = accreditationId,
            BusinessCollectionsNotes = string.Empty,
            BusinessCollectionsPercentage = 0.00M,
            CommunicationsNotes = string.Empty,
            CommunicationsPercentage = 0.20M,
            InfrastructureNotes = "Infrastructure notes testing",
            InfrastructurePercentage = 0.30M,
            MaterialName = "Plastic",
            NewMarketsNotes = "New Market Testing notes",
            NewMarketsPercentage = 0.40M,
            NewUsersRecycledPackagingWasteNotes = string.Empty,
            NewUsersRecycledPackagingWastePercentage = 0.25M,
            NotCoveredOtherCategoriesNotes = string.Empty,
            NotCoveredOtherCategoriesPercentage = 5.00M,
            OrganisationName = "",
            RecycledWasteNotes = "No recycled waste notes at this time",
            RecycledWastePercentage = 10.00M,
            SiteAddress = "To Be Confirmed",
            TaskStatus = "Reviewed",
            QueryNotes = queryNotes
        };

        return accreditationBusinessPlanDto;
    }
}