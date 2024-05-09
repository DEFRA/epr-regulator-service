namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations
{
    using Attributes;

    public class RejectRegistrationViewModel
    {
        [CharacterCount("Error.RejectionReason", "Error.RejectionReasonTooLong", 500)]
        public string? ReasonForRejection { get; set; }
    }
}
