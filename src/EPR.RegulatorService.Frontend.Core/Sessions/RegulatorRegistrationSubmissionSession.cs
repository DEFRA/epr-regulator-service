using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorRegistrationSubmissionSession
    {
        private bool _clearFilters = false;
        public List<string> Journey { get; set; } = new();
        public RegistrationSubmissionOrganisationDetails SelectedRegistration { get; set; }
        public int? CurrentPageNumber { get; set; }
        public RegistrationSubmissionsFilterModel LatestFilterChoices { get; set; }

        public bool ClearFilters
        {
            get => _clearFilters;
            set
            {
                _clearFilters = value;
                if (value)
                {
                    CurrentPageNumber = 1;
                    if (null != LatestFilterChoices)
                    {
                        LatestFilterChoices.PageNumber = 1;
                        LatestFilterChoices.OrganisationName = LatestFilterChoices.OrganisationReference = null;
                        LatestFilterChoices.OrganisationType = LatestFilterChoices.RelevantYears = LatestFilterChoices.Statuses = null;
                    }
                }
            }
        }
    }
}