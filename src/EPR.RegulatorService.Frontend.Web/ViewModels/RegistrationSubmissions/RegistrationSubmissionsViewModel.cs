namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsViewModel
    {
        public int? PageNumber { get; set; }

        public string PowerBiLogin { get; set; }
    }
}
