namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter
{
    using System;
    using System.Collections.Generic;

    using EPR.RegulatorService.Frontend.Core.Enums;

    public class RegistrationDto
    {
        public int Id { get; set; }
        public string OrganisationName { get; set; } = string.Empty;
        public string SiteAddress { get; set; } = string.Empty;
        public ApplicationOrganisationType OrganisationType { get; set; }
        public string Regulator { get; set; } = "Environment Agency (EA)"; // Default for now
    }
}
