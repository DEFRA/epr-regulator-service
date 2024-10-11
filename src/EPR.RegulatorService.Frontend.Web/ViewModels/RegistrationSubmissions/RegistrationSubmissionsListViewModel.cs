namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsListViewModel
    {
        // TODO : Data should come from the page and passed to the component
        public RegistrationSubmissionsListViewModel()
        {
                // TODO: Remove dummy data
            PagedRegistrationSubmissionList =
            [
                new Organisation { OrganisationId = "215 148",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "2222",
                    Status = "GRANTED" },
                new Organisation
                {
                    OrganisationId = "215 149",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "3333",
                    Status = "REFUSED"
                },
                new Organisation
                {
                    OrganisationId = "215 150",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "4444",
                    Status = "QUERIED"
                },
                new Organisation
                {
                    OrganisationId = "215 151",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "5555",
                    Status = "PENDING"
                },
                new Organisation
                {
                    OrganisationId = "215 152",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "6666",
                    Status = "UPDATED"
                },
                new Organisation
                {
                    OrganisationId = "215 153",
                    OrganisationName = "Aceme org Ltd",
                    ProducerType = "Small producer",
                    RefNumber = "## ### ##",
                    SubmissionDateTime = DateTime.Now,
                    SubmissionYear = "7777",
                    Status = "CANCELED"
                },
            ];
        }
        public List<Organisation> PagedRegistrationSubmissionList { get; set; }

        // TODO : Use ENUMS from common data instead of strings here
        public string GetStyleNameFromStatus(string status)
        {
            switch (status)
            {
                case "GRANTED":
                    return "govuk-tag--green";
                    break;
                case "REFUSED":
                    return "govuk-tag--red";
                    break;
                case "QUERIED":
                    return "govuk-tag--purple";
                    break;
                case "PENDING":
                    return "govuk-tag--blue";
                    break;
                case "UPDATED":
                    return "govuk-tag--yellow";
                    break;
                case "CANCELED":
                    return "govuk-tag--grey";
                    break;
                default:
                    return "govuk-tag--purple";
                    break;
            }

        }
    }

    // TODO : Replace with actual data model class. Remove dummy class
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
 
