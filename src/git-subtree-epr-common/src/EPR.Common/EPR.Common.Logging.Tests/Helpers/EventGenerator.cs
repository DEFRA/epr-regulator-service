namespace EPR.Common.Logging.Tests.Helpers;

using Constants;
using Models;

internal static class EventGenerator
{
    public static ProtectiveMonitoringEvent CreateAntivirusScanResultEvent(
        string component, Guid sessionId, string fileName, bool isFileClean, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: isFileClean ? PmcCodes.Code0212 : PmcCodes.Code0204,
            Priority: isFileClean ? Priorities.NormalEvent : Priorities.UnusualEvent,
            TransactionCode: isFileClean
                ? TransactionCodes.AntivirusCleanUpload
                : TransactionCodes.AntivirusThreatDetected,
            Message: isFileClean
                ? $"File {fileName} is clean to upload"
                : $"File {fileName} is infected and cannot be uploaded",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateFileUploadedEvent(
        string component, Guid sessionId, string fileName, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0201,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.Uploaded,
            Message: $"File {fileName} uploaded",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateFileDownloadedEvent(
        string component, Guid sessionId, string fileName, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0201,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.DownloadAllowed,
            Message: $"File {fileName} downloaded",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateSubmissionCreatedEvent(
        string component, Guid sessionId, Guid submissionId, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0104,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.SubmissionCreated,
            Message: $"Submission {submissionId} created",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateSubmissionUpdatedEvent(
        string component, Guid sessionId, Guid submissionId, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0104,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.SubmissionUpdated,
            Message: $"Submission {submissionId} updated",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateSubmissionSubmittedEvent(
        string component, Guid sessionId, Guid submissionId, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0104,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.SubmissionSubmitted,
            Message: $"Submission {submissionId} submitted",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateSubmissionApprovedEvent(
        string component, Guid sessionId, Guid submissionId, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0104,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.SubmissionApproved,
            Message: $"Submission {submissionId} approved",
            AdditionalInfo: additionalInfo);

    public static ProtectiveMonitoringEvent CreateAcceptanceTermsAndConditionsEvent(
        string component, Guid sessionId, string additionalInfo) =>
        new(
            SessionId: sessionId,
            Component: component,
            PmcCode: PmcCodes.Code0104,
            Priority: Priorities.NormalEvent,
            TransactionCode: TransactionCodes.TermsAndConditionsAccepted,
            Message: "Terms & conditions accepted",
            AdditionalInfo: additionalInfo);
}