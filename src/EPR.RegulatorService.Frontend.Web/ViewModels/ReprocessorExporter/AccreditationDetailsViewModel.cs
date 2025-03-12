using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Common.Interfaces;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter
{
    public class AccreditationDetailsViewModel : ISubmissionDetailsViewModel
    {
        public string OrganisationName { get; set; }
        public string SiteAddress { get; set; }
        public string OrganisationType { get; set; }
        public string Regulator { get; set; }

        // Additional field specific to Accreditation
        public string AccreditationType { get; set; }
    }

}
