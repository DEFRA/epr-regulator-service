using System.Text;

using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Helpers
{
    public static class ReferenceNumberPanelHelper
    {
        public static string GetPanelTitle(RegistrationSubmissionDetailsViewModel model)
        {
            if (model.Status == Core.Enums.RegistrationSubmissionStatus.Granted && model.IsResubmission)
            {
                return "RegistrationSubmissionDetails.RegistrationReferenceNumber"
                    + "|" +
                    "RegistrationSubmissionDetails.ApplicationReferenceNumber";
            }

            return !string.IsNullOrWhiteSpace(model.RegistrationReferenceNumber)
                ? "RegistrationSubmissionDetails.RegistrationReferenceNumber"
                : "RegistrationSubmissionDetails.ApplicationReferenceNumber";
        }

        public static string GetPanelContent(RegistrationSubmissionDetailsViewModel model)
        {
            if (model.Status == Core.Enums.RegistrationSubmissionStatus.Granted && model.IsResubmission)
            {
                var contentBuilder = new StringBuilder();
                contentBuilder.AppendLine(model.RegistrationReferenceNumber);
                contentBuilder.AppendLine(model.ReferenceNumber);
                return contentBuilder.ToString();
            }

            return !string.IsNullOrWhiteSpace(model.RegistrationReferenceNumber)
                ? model.RegistrationReferenceNumber
                : model.ReferenceNumber;
        }
    }
}
