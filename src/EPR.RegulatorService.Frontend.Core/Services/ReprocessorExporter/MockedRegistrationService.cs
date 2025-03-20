using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;

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
    public Registration GetRegistrationById(int id)
    {
        if (id == 99999)
        {
            throw new Exception("Mocked exception for testing purposes.");
        }

        // Determine if the registration is for an Exporter or Reprocessor
        // Even IDs return Reprocessor, Odd IDs return Exporter (mock convention)
        var organisationType = id % 2 == 0
            ? ApplicationOrganisationType.Reprocessor
            : ApplicationOrganisationType.Exporter;

        return new Registration
        {
            Id = id,
            OrganisationName = organisationType == ApplicationOrganisationType.Reprocessor
                ? "Green Ltd"  // Mock Reprocessor name
                : "Blue Exports Ltd", // Mock Exporter name
            SiteAddress = organisationType == ApplicationOrganisationType.Reprocessor
                ? "23 Ruby St, London, E12 3SE" // Only Reprocessors have a site address
                : "N/A", // Exporters do not have a site address
            OrganisationType = organisationType,
            Regulator = "Environment Agency (EA)" // Static mock value
        };
    }
}