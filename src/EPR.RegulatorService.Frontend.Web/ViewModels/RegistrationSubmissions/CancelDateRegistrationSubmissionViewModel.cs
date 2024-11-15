namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;

    public class CancelDateRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }

        [DateValidation("Date of cancellation must be a real Date",
                        "Cancellation date must include a day",
                        "Cancellation date must include a month",
                        "Cancellation date must include a year",
                        "Enter a cancellation date",
                        "Cancellate date must be a real date",
                        "Cancellation date must be after 31 December 2024")]
        public DateTime? CancellationDate { get; set; }

        public int? Day { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }
    }
}