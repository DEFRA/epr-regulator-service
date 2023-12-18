using System.ComponentModel.DataAnnotations;
using EPR.RegulatorService.Frontend.Web.Attributes;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{
    public class RejectSubmissionViewModel
    {
        public string OrganisationName { get; set; }
        public Guid SubmissionId { get; set; }
        public string SubmittedBy { get; set; }

        [CharacterCount("Error.RejectionReason", "Error.RejectionReasonTooLong", 500)]
        public string? ReasonForRejection { get; set; }

        [Required(ErrorMessage = "Error.ResubmissionRequired")]
        public bool? IsResubmissionRequired { get; set; }
    }
}
