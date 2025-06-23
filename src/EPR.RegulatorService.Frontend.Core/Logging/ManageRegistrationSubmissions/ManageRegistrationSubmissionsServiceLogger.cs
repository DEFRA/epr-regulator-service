namespace EPR.RegulatorService.Frontend.Core.Logging.ManageRegistrationSubmissions
{
    using System;
    using Microsoft.Extensions.Logging;

    public static partial class ManageRegistrationSubmissionsServiceLogger
    {
        private static readonly Action<ILogger, int?, Exception?> _getRegistrationSubmissionsFailed =
            LoggerMessage.Define<int?>(
                LogLevel.Error,
                new EventId(2001, nameof(GetRegistrationSubmissionsFailed)),
                "Failed to get registration submissions for page: {PageNumber}");

        private static readonly Action<ILogger, Guid, Exception?> _getRegistrationSubmissionDetailsFailed =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(2002, nameof(GetRegistrationSubmissionDetailsFailed)),
                "Failed to get submission details for ID: {SubmissionId}");

        private static readonly Action<ILogger, Guid, Exception?> _registrationSubmissionNotFound =
            LoggerMessage.Define<Guid>(
                LogLevel.Warning,
                new EventId(1004, nameof(RegistrationSubmissionNotFound)),
                "Submission not found for submission ID: {SubmissionId}");

        public static void GetRegistrationSubmissionsFailed(this ILogger logger, int? pageNumber, Exception ex) =>
            _getRegistrationSubmissionsFailed(logger, pageNumber, ex);

        public static void GetRegistrationSubmissionDetailsFailed(this ILogger logger, Guid submissionId, Exception ex) =>
            _getRegistrationSubmissionDetailsFailed(logger, submissionId, ex);

        public static void RegistrationSubmissionNotFound(this ILogger logger, Guid submissionId) =>
            _registrationSubmissionNotFound(logger, submissionId, null);
    }
}
