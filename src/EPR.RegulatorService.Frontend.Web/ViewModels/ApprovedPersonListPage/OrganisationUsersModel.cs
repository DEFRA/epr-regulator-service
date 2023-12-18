namespace EPR.RegulatorService.Frontend.Web.ViewModels.ApprovedPersonListPage;

using System.ComponentModel.DataAnnotations;

using Core.Models;

public class OrganisationUsersModel
{
    public Guid ExternalOrganisationId { get; set; }

    public List<OrganisationUser>? OrganisationUsers { get; set; }

   [Required(ErrorMessage = "SelectNominatedApprovedPerson")]
    public Guid? NewApprovedUserId { get; set; }
}