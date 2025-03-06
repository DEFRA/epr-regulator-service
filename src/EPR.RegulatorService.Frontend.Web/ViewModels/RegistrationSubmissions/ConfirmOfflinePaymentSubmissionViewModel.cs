namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class ConfirmOfflinePaymentSubmissionViewModel
    {
        public Guid? SubmissionId { get; set; }

        [Required(ErrorMessage = "ConfirmOfflinePaymentSubmission.ErrorMessage")]
        public bool? IsOfflinePaymentConfirmed { get; set; }

        public string? OfflinePaymentAmount { get; set; }

        public string? NationCode { get; set; }
    }
}
