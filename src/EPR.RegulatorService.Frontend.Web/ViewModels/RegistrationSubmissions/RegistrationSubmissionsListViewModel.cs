namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsListViewModel
    {
        // To Do; Data should come from the page and passed to the component
        public RegistrationSubmissionsListViewModel()
        {
            // To Do; Remove dummy data
            PagedRegistrationSubmissionList = new List<Organisation>
            {
                CreateOrganisation("215 148", "A148 org Ltd", "Small producer", "11 ### ##", "2222", "GRANTED"),
                CreateOrganisation("215 149", "A149 Ltd", "Large producer", "33 ### ##", "3333", "REFUSED"),
                CreateOrganisation("215 150", "B150 Ltd", "Small producer", "44 ### ##", "4444", "QUERIED"),
                CreateOrganisation("215 151", "Aceme org Ltd", "Small producer", "## ### ##", "5555", "PENDING"),
                CreateOrganisation("215 152", "Aceme org Ltd", "Small producer", "## ### ##", "6666", "UPDATED"),
                CreateOrganisation("215 153", "Aceme org Ltd", "Small producer", "## ### ##", "7777", "CANCELED")
            };
        }
        public List<Organisation> PagedRegistrationSubmissionList { get; set; }

        private Organisation CreateOrganisation(string id, string name, string type, string refNumber, string year, string status)
        {
            return new Organisation
            {
                OrganisationId = id,
                OrganisationName = name,
                ProducerType = type,
                RefNumber = refNumber,
                SubmissionDateTime = DateTime.Now,
                SubmissionYear = year,
                Status = status
            };
        }

        // To Do; Use ENUMS from common data instead of strings here
        public string GetStyleNameFromStatus(string status)
        {
            return status switch
            {
                "GRANTED" => "govuk-tag--green",
                "REFUSED" => "govuk-tag--red",
                "QUERIED" => "govuk-tag--purple",
                "PENDING" => "govuk-tag--blue",
                "UPDATED" => "govuk-tag--yellow",
                "CANCELED" => "govuk-tag--grey",
                _ => "govuk-tag--purple"
            };

        }
    }

    // To Do; Replace with actual data model class. Remove dummy class
    [ExcludeFromCodeCoverage]
    public class Organisation
    {
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }
        public string ProducerType { get; set; }
        public string RefNumber { get; set; }
        public DateTime SubmissionDateTime { get; set; }
        public string SubmissionYear { get; set; }
        public string Status { get; set; }
    }
}

