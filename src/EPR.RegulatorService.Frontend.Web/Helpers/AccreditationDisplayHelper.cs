namespace EPR.RegulatorService.Frontend.Web.Helpers
{
    using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

    using ViewModels.ReprocessorExporter.Accreditations;

    public static class AccreditationDisplayHelper
    {
        private static readonly HashSet<ApplicationStatus> InactiveStatuses = new()
    {
        ApplicationStatus.Started,
        ApplicationStatus.Withdrawn,
        ApplicationStatus.ReadyToSubmit
    };

        public static bool ShouldDisplayMaterial(ApplicationStatus? status, AccreditationDetailsViewModel? accreditation)
        {
            if (status is null || IsInactiveStatus(status.Value))
            {
                return false;
            }

            return accreditation is not null && ShouldDisplayAccreditation(accreditation.Status);
        }

        public static bool ShouldDisplayAccreditation(ApplicationStatus status)
        {
            return !IsInactiveStatus(status);
        }

        private static bool IsInactiveStatus(ApplicationStatus status)
        {
            return InactiveStatuses.Contains(status);
        }
    }
}
