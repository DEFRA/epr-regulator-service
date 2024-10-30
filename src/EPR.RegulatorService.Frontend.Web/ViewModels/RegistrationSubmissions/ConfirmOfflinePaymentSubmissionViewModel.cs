namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class ConfirmOfflinePaymentSubmissionViewModel
    {
        public Guid? OrganisationId { get; set; }

        [Required(ErrorMessage = "ErrorMessage")]
        public bool? IsOfflinePaymentConfirmed { get; set; }
    }
}
