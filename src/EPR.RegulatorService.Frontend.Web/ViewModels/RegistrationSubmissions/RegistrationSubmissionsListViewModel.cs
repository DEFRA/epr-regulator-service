
using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;


[ExcludeFromCodeCoverage]
public class RegistrationSubmissionsListViewModel
{
    // To Do; Data should come from the page and passed to the component
    public RegistrationSubmissionsListViewModel()
    {
        // To Do; Remove dummy data
        PagedRegistrationSubmissionList = new List<Organisation>
        {
            CreateOrganisation("215 151", "Aceme org Ltd", RegistrationSubmissionOrganisationType.large, "## ### ##", "5555", RegistrationSubmissionStatus.pending),
            CreateOrganisation("215 148", "A148 org Ltd", RegistrationSubmissionOrganisationType.compliance, "11 ### ##", "2222", RegistrationSubmissionStatus.granted),
            CreateOrganisation("215 149", "A149 Ltd", RegistrationSubmissionOrganisationType.small, "33 ### ##", "3333", RegistrationSubmissionStatus.refused),
            CreateOrganisation("215 150", "B150 Ltd", RegistrationSubmissionOrganisationType.small, "44 ### ##", "4444", RegistrationSubmissionStatus.queried),
            CreateOrganisation("215 152", "Aceme org Ltd", RegistrationSubmissionOrganisationType.compliance, "## ### ##", "6666", RegistrationSubmissionStatus.updated),
            CreateOrganisation("215 153", "Aceme org Ltd", RegistrationSubmissionOrganisationType.large, "## ### ##", "7777", RegistrationSubmissionStatus.cancelled),
            CreateOrganisation("215 148", "A148 org Ltd", RegistrationSubmissionOrganisationType.compliance, "11 ### ##", "2222", RegistrationSubmissionStatus.granted),
            CreateOrganisation("215 149", "A149 Ltd", RegistrationSubmissionOrganisationType.small, "33 ### ##", "3333", RegistrationSubmissionStatus.refused),
            CreateOrganisation("215 150", "B150 Ltd", RegistrationSubmissionOrganisationType.small, "44 ### ##", "4444", RegistrationSubmissionStatus.queried),
            CreateOrganisation("215 152", "Aceme org Ltd", RegistrationSubmissionOrganisationType.compliance, "## ### ##", "6666", RegistrationSubmissionStatus.updated),
            CreateOrganisation("215 153", "Aceme org Ltd", RegistrationSubmissionOrganisationType.large, "## ### ##", "7777", RegistrationSubmissionStatus.cancelled),
            CreateOrganisation("215 148", "A148 org Ltd", RegistrationSubmissionOrganisationType.compliance, "11 ### ##", "2222", RegistrationSubmissionStatus.granted),
            CreateOrganisation("215 149", "A149 Ltd", RegistrationSubmissionOrganisationType.small, "33 ### ##", "3333", RegistrationSubmissionStatus.refused),
            CreateOrganisation("215 150", "B150 Ltd", RegistrationSubmissionOrganisationType.small, "44 ### ##", "4444", RegistrationSubmissionStatus.queried),
            CreateOrganisation("215 152", "Aceme org Ltd", RegistrationSubmissionOrganisationType.compliance, "## ### ##", "6666", RegistrationSubmissionStatus.updated),
            CreateOrganisation("215 153", "Aceme org Ltd", RegistrationSubmissionOrganisationType.large, "## ### ##", "7777", RegistrationSubmissionStatus.cancelled)
        };
    }
    public List<Organisation> PagedRegistrationSubmissionList { get; set; }

    private Organisation CreateOrganisation(string id, string name, RegistrationSubmissionOrganisationType type, string refNumber, string year, RegistrationSubmissionStatus status)
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
    public string GetStyleNameFromStatus(RegistrationSubmissionStatus status )
    {
        return status switch
        {
            RegistrationSubmissionStatus.granted => "govuk-tag--green",
            RegistrationSubmissionStatus.refused => "govuk-tag--red",
            RegistrationSubmissionStatus.queried => "govuk-tag--purple",
            RegistrationSubmissionStatus.pending => "govuk-tag--blue",
            RegistrationSubmissionStatus.updated => "govuk-tag--yellow",
            RegistrationSubmissionStatus.cancelled => "govuk-tag--grey",
            RegistrationSubmissionStatus.none => "govuk-tag--purple",
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
    public RegistrationSubmissionOrganisationType ProducerType { get; set; }
    public string RefNumber { get; set; }
    public DateTime SubmissionDateTime { get; set; }
    public string SubmissionYear { get; set; }
    public RegistrationSubmissionStatus Status { get; set; }
}

