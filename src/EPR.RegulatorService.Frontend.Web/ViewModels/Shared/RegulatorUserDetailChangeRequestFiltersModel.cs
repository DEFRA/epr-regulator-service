namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public class RegulatorUserDetailChangeRequestFiltersModel
{
    public string? SearchOrganisationName { get; set; }

    public bool IsApprovedUserTypeChecked { get; set; }

    public bool IsDelegatedUserTypeChecked { get; set; }
}