using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorRegistrationSubmissionSession
    {
        public List<string> Journey { get; set; } = new();
        public RegistrationSubmissionOrganisationDetails SelectedRegistration { get; set; }
        public int? CurrentPageNumber { get; set; }
        public RegistrationSubmissionsFilterModel LatestFilterChoices { get; set; }

        public bool ClearFilters { get; set; }
    }
}