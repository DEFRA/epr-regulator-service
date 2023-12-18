namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Core.Models;

public class EnrolmentRequestDetailsModel
{
    public bool IsApprovedUserAccepted { get; set; }
    public User ApprovedUser { get; set; }
    public List<User> DelegatedUsers { get; set; }
}