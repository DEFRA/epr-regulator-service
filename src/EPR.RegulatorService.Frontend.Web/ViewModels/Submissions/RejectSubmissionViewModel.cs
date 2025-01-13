using EPR.RegulatorService.Frontend.Web.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{
    [ExcludeFromCodeCoverage]
    public class RejectSubmissionViewModel : IValidatableObject
    {
        public string? SubmittedBy { get; set; }
        public bool SubmissionRejected { get; set; }

        [CharacterCount("Error.RejectionReason", "Error.RejectionReasonTooLong", 500)]
        public string? ReasonForRejection { get; set; }

        public bool? IsResubmissionRequired { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SubmissionRejected && IsResubmissionRequired == null)
            {
                yield return new ValidationResult(
                    "Error.ResubmissionRequired",
                    new[] { nameof(IsResubmissionRequired) }
                );
            }
        }
    }
}
