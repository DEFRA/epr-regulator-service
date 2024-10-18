namespace EPR.RegulatorService.Frontend.Web.Helpers
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    public static class ReferenceNumberPanelHelper
    {
        public static string GetPanelTitle(RegistrationSubmissionDetailsViewModel model) => model.Status == RegistrationSubmissionStatus.granted
                ? "RegistrationSubmissionDetails.RegistrationReferenceNumber"
                : "RegistrationSubmissionDetails.ApplicationReferenceNumber";
        public static string GetPanelContent(RegistrationSubmissionDetailsViewModel model) => model.Status == RegistrationSubmissionStatus.granted
            ? model.RegistrationReferenceNumber
            : model.ApplicationReferenceNumber;
    }
}
