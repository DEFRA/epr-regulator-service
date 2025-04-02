using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

/// <summary>
/// This service provides mock registration data for testing purposes.
/// It follows a simple convention:
/// - If the ID is even, the organisation is a Reprocessor.
/// - If the ID is odd, the organisation is an Exporter.
/// This logic is purely for testing and will be replaced with real database queries.
/// </summary>
public class MockedRegistrationService : IRegistrationService
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

    public Task<RegistrationMaterial> GetRegistrationMaterialAsync(int registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.Materials)
            .FirstOrDefault(rm => rm.Id == registrationMaterialId);

        if (registrationMaterial == null)
        {
            throw new NotFoundException("Registration material not found.");
        }

        return Task.FromResult(registrationMaterial);
    }

    public async Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, ApplicationStatus? status, string? comments)
    {
        var registrationMaterial = await GetRegistrationMaterialAsync(registrationMaterialId);
        var registration = _registrations.First(r => r.Id == registrationMaterial.RegistrationId);

        registration.Materials.Remove(registrationMaterial);

        string? registrationNumber = status == ApplicationStatus.Granted ? "ABC123 4563" : null;
        string? statusUpdatedBy = status != null ? "Test User" : null;
        DateTime? statusUpdatedAt = status != null ? DateTime.Now : null;

        var updatedRegistrationMaterial = CreateRegistrationMaterial(
            registrationMaterial.RegistrationId,
            registrationMaterial.MaterialName,
            registration.OrganisationType,
            status,
            statusUpdatedBy,
            statusUpdatedAt,
            registrationNumber);

        registration.Materials.Add(updatedRegistrationMaterial);
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
                OrganisationName = "Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = organisationType,
                Regulator = "Environment Agency (EA)",
                Tasks = CreateRegistrationTasks(registrationId, organisationType),
                Materials =
                [
                    CreateRegistrationMaterial(registrationId, "Plastic", organisationType)
                ]
            };
    }

    private static Registration CreateExporterRegistration(int registrationId)
    {
        const ApplicationOrganisationType organisationType = ApplicationOrganisationType.Exporter;

        return new Registration
        {
            Id = registrationId,
            OrganisationName = "Blue Exports Ltd",
            SiteAddress = "N/A",
            OrganisationType = organisationType,
            Regulator = "Environment Agency (EA)",
            Tasks = CreateRegistrationTasks(registrationId, organisationType),
            Materials =
            [
                CreateRegistrationMaterial(registrationId, "Plastic", organisationType)
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
                new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.Completed, TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite }
            ];
        }

        return
        [
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.BusinessAddress },
            new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.Completed, TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions }
        ];
    }

#pragma warning disable S107 - Ignoring the number of parameters in this method as it is required for testing purposes
    private static RegistrationMaterial CreateRegistrationMaterial(
        int registrationId,
        string materialName,
        ApplicationOrganisationType organisationType,
        ApplicationStatus? status = null,
        string? statusUpdatedBy = null,
        DateTime? statusUpdatedAt = null,
        string? registrationNumber = null)
#pragma warning restore S107
    {
        int registrationMaterialId = registrationId * 10;

        return new RegistrationMaterial
        {
            Id = registrationMaterialId,
            RegistrationId = registrationId,
            MaterialName = materialName,
            DeterminationDate = organisationType == ApplicationOrganisationType.Reprocessor ? DateTime.Now.AddDays(-7) : null,
            Status = status,
            StatusUpdatedByName = statusUpdatedBy,
            StatusUpdatedAt = statusUpdatedAt,
            RegistrationNumber = registrationNumber,
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
                new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.Completed, TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs },
                new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
            ];
        }

        return
        [
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.MaterialDetailsAndContact },
            new RegistrationTask { Id = taskId++, Status = RegulatorTaskStatus.Completed, TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails},
            new RegistrationTask { Id = taskId, Status = RegulatorTaskStatus.NotStarted, TaskName = RegulatorTaskType.SamplingAndInspectionPlan }
        ];
    }
}