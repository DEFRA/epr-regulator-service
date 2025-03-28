namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.ComponentModel.DataAnnotations;

public class GrantRegistrationSubmissionViewModel
{
    public Guid SubmissionId { get; set; } 

    [Required(ErrorMessage = "ErrorMessage")]
    public bool? IsGrantRegistrationConfirmed { get; set; }

    public bool IsResubmission { get; set; }
    public string? ResubmissionFileId { get; internal set; }
}