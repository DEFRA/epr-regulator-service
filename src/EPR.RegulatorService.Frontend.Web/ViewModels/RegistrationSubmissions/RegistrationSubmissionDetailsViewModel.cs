namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;

    public class RegistrationSubmissionDetailsViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationReference { get; set; }

        public string OrganisationName { get; set; }

        public string? ApplicationReferenceNumber { get; set; }

        public string? RegistrationReferenceNumber { get; set; }

        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

        public BusinessAddress BusinessAddress { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public string RegisteredNation { get; set; }

        public string PowerBiLogin { get; set; }

        public RegistrationSubmissionStatus Status { get; set; }

        public SubmissionDetailsViewModel SubmissionDetails { get; set; }
    }
}
