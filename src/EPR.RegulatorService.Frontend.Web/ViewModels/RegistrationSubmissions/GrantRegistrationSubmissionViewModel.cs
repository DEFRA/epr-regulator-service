namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.ComponentModel.DataAnnotations;

public class GrantRegistrationSubmissionViewModel
{
    public Guid SubmissionId { get; set; } 

    [Required(ErrorMessage = "ErrorMessage")]
    public bool? IsGrantRegistrationConfirmed { get; set; }
}
