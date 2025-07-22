using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorRegistrationSubmissionSession
    {
        private bool _clearFilters = false;

        public List<string> Journey { get; set; } = [];

        public IDictionary<Guid, RegistrationSubmissionOrganisationDetails> SelectedRegistrations { get; set; } = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>();
        public IDictionary<Guid, RegistrationSubmissionOrganisationType> SelectedOrganisationTypes { get; set; } = new Dictionary<Guid, RegistrationSubmissionOrganisationType>();
        public int? CurrentPageNumber { get; set; }

        public RegistrationSubmissionsFilterModel LatestFilterChoices { get; set; }

        public IDictionary<Guid, RegistrationSubmissionOrganisationDetails> OrganisationDetailsChangeHistory { get; set; } = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>();

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
                        LatestFilterChoices.OrganisationType = LatestFilterChoices.RelevantYears = LatestFilterChoices.Statuses = LatestFilterChoices.ResubmissionStatuses = null;
                    }
                }
            }
        }

        public string FileDownloadRequestType { get; set; }
    }
}