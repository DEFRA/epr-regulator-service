namespace EPR.RegulatorService.Frontend.Web.ViewModels.InviteNewApprovedPerson;

public class ConfirmationModel
{
    public bool? HasANewApprovedPersonBeenNominated { get; set; }

    public string InvitedApprovedPersonFullName { get; set; }

    public string InvitedApprovedPersonEmail { get; set; }
}