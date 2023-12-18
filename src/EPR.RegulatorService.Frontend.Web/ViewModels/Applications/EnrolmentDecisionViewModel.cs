using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications;

[ExcludeFromCodeCoverage]
public class EnrolmentDecisionViewModel
{
    public string RejectedUserFirstName { get; set; }
    public string RejectedUserLastName { get; set; }
    public string ApprovedUserFirstName { get; set; }
    public string ApprovedUserLastName { get; set; }
    public Guid OrganisationId { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Reject.CommentsMaxLengthError")]
    public string? Comments { get; set; }
}