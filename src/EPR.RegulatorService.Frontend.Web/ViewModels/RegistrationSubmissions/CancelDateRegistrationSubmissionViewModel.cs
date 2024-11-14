namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    using EPR.RegulatorService.Frontend.Web.Attributes;

    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class CancelDateRegistrationSubmissionViewModel
    {

        public Guid SubmissionId { get; set; }

        [DateValidation("Enter a cancellation date", "Cancellation date must be after 31 December 2024")]
        public DateTime? CancellationDate { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Cancellation date must include a Day")]
        [Range(1, 31, ErrorMessage = "Date of cancellation must be a real Day")]
        public int? Day { get; set; }

        [Required(ErrorMessage = "Cancellation date must include a Month")]
        [Range(1, 12, ErrorMessage = "Date of cancellation must be a real Month")]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Cancellation date must include a Year")]
        [Range(2024, 9999, ErrorMessage = "Date of cancellation must be a real Year")]
        public int? Year { get; set; }
    }
}