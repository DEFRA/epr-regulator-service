namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    public class CancelDateRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }
        public DateTime? CancellationDate { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}