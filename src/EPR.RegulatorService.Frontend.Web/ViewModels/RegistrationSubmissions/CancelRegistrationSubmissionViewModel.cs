namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;

    public class CancelRegistrationSubmissionViewModel
    {
        public Guid OrganisationId { get; set; }

        [CharacterCount("Error.CancellationReason", "Error.CancellationReasonTooLong", 400)]
        public string? CancellationReason { get; set; }

    }
}
