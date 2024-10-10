namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsViewModel
    {
        public string PowerBiLogin { get; set; }

        // TODO : Data to be populated from applied filters.
        // TODO: Check naming standards 
        public RegistrationSubmissionsListViewModel FilteredDataList { get; set; }
    }
}
