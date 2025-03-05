using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter
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
