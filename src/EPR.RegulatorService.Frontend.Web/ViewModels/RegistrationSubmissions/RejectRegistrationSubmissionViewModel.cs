namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;
    using EPR.RegulatorService.Frontend.Web.Constants;

    public class RejectRegistrationSubmissionViewModel
    {
        [CharacterCount("Error.Reject", "Error.RejectReasonTooLong", 400)]
        public string? RejectReason { get; set; }

        public string BackToAllSubmissionsUrl { get; set; } = PagePath.RegistrationSubmissionsRoute;
    }
}
