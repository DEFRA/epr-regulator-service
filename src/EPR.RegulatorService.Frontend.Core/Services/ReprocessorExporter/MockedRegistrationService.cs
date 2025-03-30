using EPR.RegulatorService.Frontend.Core.Enums;
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

        var registration = _registrations.FirstOrDefault(r => r.OrganisationType == organisationType);

        if (registration == null)
        {
            registration = organisationType == ApplicationOrganisationType.Reprocessor
                ? CreateReprocessorRegistration(id)
                : CreateExporterRegistration(id);

            _registrations.Add(registration);
        }

        return Task.FromResult(registration);
    }

    public Task<RegistrationMaterial> GetRegistrationMaterial(int registrationMaterialId)
    {
        var registrationMaterial = _registrations.SelectMany(r => r.RegistrationMaterials)
            .FirstOrDefault(rm => rm.Id == registrationMaterialId);

        if (registrationMaterial == null)
        {
            throw new NotFoundException("Registration material not found.");
        }

        return Task.FromResult(registrationMaterial);
    }

    public async Task SaveRegistrationMaterialStatus(int registrationMaterialId, ApplicationStatus? status, string? comments)
    {
        var registrationMaterial = await GetRegistrationMaterial(registrationMaterialId);
        var registration = _registrations.First(r => r.Id == registrationMaterial.RegistrationId);

        registration.RegistrationMaterials.Remove(registrationMaterial);

        var updatedRegistrationMaterial = CreateRegistrationMaterial(registrationMaterial.RegistrationId, registrationMaterial.MaterialName, status, comments);
        registration.RegistrationMaterials.Add(updatedRegistrationMaterial);
    }

    private static List<Registration> SeedRegistrations() =>
    [
        CreateExporterRegistration(1),
        CreateReprocessorRegistration(2)
    ];

    private static Registration CreateReprocessorRegistration(int registrationId) => new()
    {
        Id = registrationId,
        OrganisationName = "Green Ltd",
        SiteAddress = "23 Ruby St, London, E12 3SE",
        OrganisationType = ApplicationOrganisationType.Reprocessor,
        Regulator = "Environment Agency (EA)",
        RegistrationMaterials =
        [
            CreateRegistrationMaterial(registrationId, "Plastic")
        ]
    };

    private static Registration CreateExporterRegistration(int registrationId) => new()
    {
        Id = registrationId,
        OrganisationName = "Blue Exports Ltd",
        SiteAddress = "N/A",
        OrganisationType = ApplicationOrganisationType.Exporter,
        Regulator = "Environment Agency (EA)",
        RegistrationMaterials =
        [
            CreateRegistrationMaterial(registrationId, "Plastic")
        ]
    };

    private static RegistrationMaterial CreateRegistrationMaterial(
        int registrationId,
        string materialName,
        ApplicationStatus? status = null,
        string? comments = null)
        => new()
        {
            Id = registrationId * 10,
            RegistrationId = registrationId,
            MaterialName = materialName,
            Status = status,
            Comments = comments
        };
}