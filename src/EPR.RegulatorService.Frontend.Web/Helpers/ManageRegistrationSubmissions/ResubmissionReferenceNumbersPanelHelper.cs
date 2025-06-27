namespace EPR.RegulatorService.Frontend.Web.Helpers.ManageRegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

    public static class ResubmissionReferenceNumbersPanelHelper
    {
        public static string GetProducerRegistrationReferencePanelTitle(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ProducerRegistrationNumber)
                ? "ProducerRegistrationNumber"
                : string.Empty;

        public static string GetApplicationReferencePanelTitle(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ApplicationReferenceNumber)
                ? "ApplicationReferenceNumber"
                : string.Empty;

        public static string GetProducerRegistrationReferencePanelContent(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ProducerRegistrationNumber)
            ? model.ProducerRegistrationNumber
            : string.Empty;

        public static string GetApplicationReferencePanelContent(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ApplicationReferenceNumber)
            ? model.ApplicationReferenceNumber
            : string.Empty;
    }
}
