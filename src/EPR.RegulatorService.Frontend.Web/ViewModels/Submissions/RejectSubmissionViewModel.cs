using EPR.RegulatorService.Frontend.Web.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{
    public class RejectSubmissionViewModel
    {
        public string? SubmittedBy { get; set; }

        [CharacterCount("Error.RejectionReason", "Error.RejectionReasonTooLong", 500)]
        public string? ReasonForRejection { get; set; }

        [Required(ErrorMessage = "Error.ResubmissionRequired")]
        public bool? IsResubmissionRequired { get; set; }
    }
}
