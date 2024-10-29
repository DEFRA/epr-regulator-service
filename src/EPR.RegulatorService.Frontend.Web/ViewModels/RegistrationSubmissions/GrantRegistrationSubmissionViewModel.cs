namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;
    using EPR.RegulatorService.Frontend.Web.Constants;

    public class GrantRegistrationSubmissionViewModel
    {
        [CharacterCount("Error.Query", "Error.QueryTooLong", 400)]
        public string? Query { get; set; }

        public string BackToAllSubmissionsUrl { get; set; } = PagePath.RegistrationSubmissionsRoute;
    }
}
