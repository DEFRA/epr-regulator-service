namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using System.ComponentModel.DataAnnotations;

public class AcceptRegistrationSubmissionViewModel
{
    [Required(ErrorMessage = "Error.AcceptSubmission")]
    public bool? Accepted { get; set; }
}
