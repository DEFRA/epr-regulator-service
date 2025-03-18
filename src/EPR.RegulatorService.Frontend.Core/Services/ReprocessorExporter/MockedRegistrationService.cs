namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;

    public class MockedRegistrationService : IRegistrationService
    {
        public RegistrationDto? GetRegistrationById(int id)
        {
            // Determine if the registration is for an Exporter or Reprocessor
            var organisationType = id % 2 == 0
                ? ApplicationOrganisationType.Reprocessor
                : ApplicationOrganisationType.Exporter;

            return new RegistrationDto
            {
                Id = id,
                OrganisationName = organisationType == ApplicationOrganisationType.Reprocessor
                    ? "Green Ltd"
                    : "Blue Exports Ltd",
                SiteAddress = organisationType == ApplicationOrganisationType.Reprocessor
                    ? "23 Ruby St, London, E12 3SE"
                    : "N/A", // Exporters do not have a site address
                OrganisationType = organisationType,
                Regulator = "Environment Agency (EA)"
            };
        }
    }
}