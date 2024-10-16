namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Models;

    public class OrganisationRegistrationDetailsViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationReference { get; set; }

        public string OrganisationName { get; set; }

        public string RegistrationReferenceNumber { get; set; }

        public string OrganisationType { get; set; }

        public BusinessAddress BusinessAddress { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public string RegisteredNation { get; set; }

        public string PowerBiLogin { get; set; }
    }
}
