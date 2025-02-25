namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class CancelRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }

        [StringLength(400, ErrorMessage = "Error.CancellationReasonTooLong")]
        public string? CancellationReason { get; set; }

    }
}
