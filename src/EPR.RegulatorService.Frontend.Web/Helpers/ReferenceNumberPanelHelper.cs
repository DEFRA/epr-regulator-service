namespace EPR.RegulatorService.Frontend.Web.Helpers
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    public static class ReferenceNumberPanelHelper
    {
        public static string GetPanelTitle(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.RegistrationReferenceNumber)
                ? "RegistrationSubmissionDetails.RegistrationReferenceNumber"
                : "RegistrationSubmissionDetails.ApplicationReferenceNumber";
        public static string GetPanelContent(RegistrationSubmissionDetailsViewModel model) => !string.IsNullOrWhiteSpace(model.RegistrationReferenceNumber)
            ? model.RegistrationReferenceNumber
            : model.ApplicationReferenceNumber;
    }
}
