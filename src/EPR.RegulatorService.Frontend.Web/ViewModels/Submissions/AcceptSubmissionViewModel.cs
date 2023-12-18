namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using System.ComponentModel.DataAnnotations;

public class AcceptSubmissionViewModel
{
    public string OrganisationName { get; set; }
    
    [Required(ErrorMessage = "Error.AcceptSubmission")]
    public bool? Accepted { get; set; } 
}
