namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsViewModel
    {
        public string PowerBiLogin { get; set; }

        // TO DO ; Data to be populated from applied filters.
        // TO DO ; Check naming standards 
        public RegistrationSubmissionsListViewModel FilteredDataList { get; set; }
    }
}
