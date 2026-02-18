namespace EPR.Common.Logging.Constants;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class TransactionCodes
{
    /// <summary>
    /// File Not Found (HTTP 404)
    /// Attempts to request routes which do not exist
    /// </summary>
    public const string Epr404 = "EPR_404";

    /// <summary>
    /// Application Error (HTTP 5xx)
    /// </summary>
    public const string Epr5xx = "EPR_5XX";

    /// <summary>
    /// File submitted for anti-virus check
    /// </summary>
    public const string AntivirusCheck = "EPR_ANTIVIRUS_CHECK";

    /// <summary>
    /// Anti-virus service identified not threats in a file
    /// </summary>
    public const string AntivirusCleanUpload = "EPR_ANTIVIRUS_CLEAN_UPLOAD";

    /// <summary>
    /// Anti-virus service identified a threat in a file
    /// </summary>
    public const string AntivirusThreatDetected = "EPR_ANTIVIRUS_THREAT_DETECTED";

    /// <summary>
    /// A file has been uploaded
    /// </summary>
    public const string Uploaded = "EPR_UPLOADED";

    /// <summary>
    /// File download denied
    /// Download blocked as user unauthorised
    /// </summary>
    public const string DownloadBlocked = "EPR_DOWNLOAD_BLOCKED";

    /// <summary>
    /// File download allowed
    /// </summary>
    public const string DownloadAllowed = "EPR_DOWNLOAD_ALLOWED";

    /// <summary>
    /// File validation failed
    /// When attempting to validate the contents of a file an error occurred
    /// </summary>
    public const string FileValidationFailed = "EPR_FILE_INVALID";
    
    /// <summary>
    /// Terms & Conditions accepted by user
    /// </summary>
    public const string TermsAndConditionsAccepted = "EPR_TC_ACCEPTED";

    /// <summary>
    /// Initial submission created (PoM/Registration Data)
    /// </summary>
    public const string SubmissionCreated = "EPR_SUBMISSION_CREATED";

    /// <summary>
    /// Submission updated (e.g. validation result, additional file uploaded)
    /// </summary>
    public const string SubmissionUpdated = "EPR_SUBMISSION_UPDATED";

    /// <summary>
    /// Submission Submitted by CS/Producer
    /// </summary>
    public const string SubmissionSubmitted = "EPR_SUBMISSION_SUBMITTED";

    /// <summary>
    /// Submission approved by regulator
    /// </summary>
    public const string SubmissionApproved = "EPR_SUBMISSION_APPROVED";

    /// <summary>
    /// Scheme member removed from compliance scheme
    /// </summary>
    public const string SchemeMemberRemoved = "RPD_REMOVE_SCHEME_MEMBERSHIP";
}