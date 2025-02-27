namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;

    public interface ISubmissionDetails
    {
        string OrganisationName { get; }
        string OrganisationReference { get; }
        DateTime RegistrationDateTime { get; }
        int RegistrationYear { get; }
        RegistrationSubmissionStatus Status { get; }
        RegistrationSubmissionOrganisationType OrganisationType { get; }
    }
}