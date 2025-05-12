using EPR.RegulatorService.Frontend.Web.Attributes;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{
    [ExcludeFromCodeCoverage]
    public class RejectSubmissionViewModel
    {
        public int SubmissionHash { get; set; }

        public string? SubmittedBy { get; set; }

        [CharacterCount("Error.RejectionReason", "Error.RejectionReasonTooLong", 500)]
        public string? ReasonForRejection { get; set; }

        [Required(ErrorMessage = "Error.ResubmissionRequired")]
        public bool? IsResubmissionRequired { get; set; }
    }
}
