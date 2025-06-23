namespace EPR.RegulatorService.Frontend.Web.Logging.ManageRegistrationSubmissions
{
    public static partial class ManageRegistrationSubmissionsApiClientLogger
    {
        private static readonly Action<ILogger, Guid, Exception?> _httpRequestFailed =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(1001, nameof(HttpRequestFailed)),
                "HTTP request failed for submission ID: {SubmissionId}");

        private static readonly Action<ILogger, Guid, Exception?> _deserializationFailed =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(1002, nameof(DeserializationFailed)),
                "Deserialization failed for submission ID: {SubmissionId}");

        private static readonly Action<ILogger, Guid, Exception?> _unexpectedError =
            LoggerMessage.Define<Guid>(
                LogLevel.Error,
                new EventId(1003, nameof(UnexpectedError)),
                "Unexpected error occurred for submission ID: {SubmissionId}");

        private static readonly Action<ILogger, Guid, Exception?> _registrationSubmissionNotFound =
            LoggerMessage.Define<Guid>(
                LogLevel.Warning,
                new EventId(1004, nameof(RegistrationSubmissionNotFound)),
                "Submission not found for submission ID: {SubmissionId}");

        public static void HttpRequestFailed(this ILogger logger, Guid submissionId, Exception ex) =>
            _httpRequestFailed(logger, submissionId, ex);

        public static void DeserializationFailed(this ILogger logger, Guid submissionId, Exception ex) =>
            _deserializationFailed(logger, submissionId, ex);

        public static void UnexpectedError(this ILogger logger, Guid submissionId, Exception ex) =>
            _unexpectedError(logger, submissionId, ex);

        public static void RegistrationSubmissionNotFound(this ILogger logger, Guid submissionId) =>
            _registrationSubmissionNotFound(logger, submissionId, null);
    }
}
