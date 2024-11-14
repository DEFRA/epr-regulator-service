namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;

    public class RejectRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }

        [CharacterCount("Error.Reject", "Error.RejectReasonTooLong", 400)]
        public string? RejectReason { get; set; }
    }
}
