using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.Extensions.Logging;

namespace EPR.RegulatorService.Frontend.Core.Services;

public static partial class FacadeServiceLoggerMessages
{
    private static readonly Func<ILogger, RegistrationSubmissionsFilterModel, IDisposable?> _beginRegistrationSubmissionsScopeDelegate =
        LoggerMessage.DefineScope<RegistrationSubmissionsFilterModel>("{@Filters}");

    public static IDisposable? BeginRegistrationSubmissionsScope(this ILogger logger, RegistrationSubmissionsFilterModel filters) =>
        _beginRegistrationSubmissionsScopeDelegate(logger, filters);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "An error occurred while processing a message: {ErrorMessage}")]
    public static partial void LogFacadeServiceError(this ILogger logger, Exception exception, string errorMessage);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Retrieving Registration Submissions from facade API path: {FacadePath}")]
    public static partial void LogRetrievingRegistrationSubmissions(this ILogger logger, string facadePath);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Successfully retrieved {SubmissionCount} registration submissions from facade API out of a total of {SubmissionCountTotal}")]
    public static partial void LogRetrievingRegistrationSubmissionsSuccess(this ILogger logger, int submissionCount, int submissionCountTotal);
}