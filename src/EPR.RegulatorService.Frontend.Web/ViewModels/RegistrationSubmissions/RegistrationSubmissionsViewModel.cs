namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsViewModel
    {
        public RegistrationSubmissionsListViewModel ListViewModel { get; set; }

        public string PowerBiLogin { get; set; }

        public string? AgencyName { get; set; }
    }
}
