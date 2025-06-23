namespace EPR.RegulatorService.Frontend.Web.Logging.ManageRegistrationSubmissions
{
    internal static partial class ManageRegistrationSubmissionsControllerLogging
    {
        private static readonly Action<ILogger, int?, Exception?> _logErrorFetchingSubmissions =
            LoggerMessage.Define<int?>(
                LogLevel.Error,
                new EventId(1001, nameof(ErrorFetchingSubmissions)),
                "Error fetching registration submissions for page {PageNumber}");

        private static readonly Action<ILogger, Guid, Exception?> _logHttpErrorFetchingSubmissionDetails =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(1002, nameof(HttpErrorFetchingSubmissionDetails)),
                "HTTP error retrieving submission {SubmissionId}");

        private static readonly Action<ILogger, Guid, Exception?> _logUnexpectedErrorFetchingSubmissionDetails =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(1003, nameof(UnexpectedErrorFetchingSubmissionDetails)),
                "Unexpected error retrieving submission {SubmissionId}");

        public static void ErrorFetchingSubmissions(this ILogger logger, int? pageNumber, Exception? ex) =>
            _logErrorFetchingSubmissions(logger, pageNumber, ex);

        public static void HttpErrorFetchingSubmissionDetails(this ILogger logger, Guid submissionId, Exception? ex) =>
            _logHttpErrorFetchingSubmissionDetails(logger, submissionId, ex);

        public static void UnexpectedErrorFetchingSubmissionDetails(this ILogger logger, Guid submissionId, Exception? ex) =>
            _logUnexpectedErrorFetchingSubmissionDetails(logger, submissionId, ex);
    }
}
