namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class ConfirmRegistrationRefusalViewModel
    {
        public Guid? SubmissionId { get; set; }

        [Required(ErrorMessage = "ConfirmRegistrationRefusal.ErrorMessage")]
        public bool? IsRegistrationRefusalConfirmed { get; set; }

        public string? RejectReason { get; set; }

    }
}
