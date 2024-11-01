namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.ComponentModel.DataAnnotations;

public class GrantRegistrationSubmissionViewModel
{
    public Guid OrganisationId { get; set; }

    [Required(ErrorMessage = "ErrorMessage")]
    public bool? IsGrantRegistrationConfirmed { get; set; }

}