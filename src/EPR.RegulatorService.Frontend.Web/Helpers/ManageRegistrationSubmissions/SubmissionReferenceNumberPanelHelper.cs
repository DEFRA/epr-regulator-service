using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Helpers.ManageRegistrationSubmissions
{
    public static class SubmissionReferenceNumberPanelHelper
    {
        public static string GetPanelTitle(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ProducerRegistrationNumber)
                ? "ProducerRegistrationNumber"
                : "ApplicationReferenceNumber";

        public static string GetPanelContent(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.ProducerRegistrationNumber)
            ? model.ProducerRegistrationNumber
            : model.ApplicationReferenceNumber;
    }
}
