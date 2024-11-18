namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes;

    public class CancelDateRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }

        [DateValidation("Error.InvalidDateErrorMessage",
                        "Error.MissingDayErrorMessage",
                        "Error.MissingMonthErrorMessage",
                        "Error.MissingYearErrorMessage",
                        "Error.EmptyDateErrorMessage",
                        "Error.InvalidRealDateErrorMessage",
                        "Error.PastDateErrorMessage")]
        public DateTime? CancellationDate { get; set; }

        public int? Day { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }
    }
}