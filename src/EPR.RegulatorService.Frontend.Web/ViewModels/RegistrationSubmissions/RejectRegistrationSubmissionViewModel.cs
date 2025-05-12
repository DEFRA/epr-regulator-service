namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class RejectRegistrationSubmissionViewModel
    {
        public string SubmissionId { get; set; }

        [StringLength(400, ErrorMessage = "Error.RejectReasonTooLong")]
        public string? RejectReason { get; set; }

        public bool IsResubmission { get; set; }

        public string? ResubmissionFileId { get; set; }
    }
}
