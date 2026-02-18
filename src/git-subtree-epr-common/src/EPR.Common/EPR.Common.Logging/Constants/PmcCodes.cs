namespace EPR.Common.Logging.Constants;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Constants used for Protective Monitoring Codes.
/// Source: from DEFRA Protective Monitoring Specification.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PmcCodes
{
    /// <summary>
    /// A master clock must be in place which is synchronized to an atomic clock.
    /// </summary>
    public const string Code0101 = "01-01";

    /// <summary>
    /// All log file records must be date and time stamped using Universal Coordinated Time
    /// and must contain a log file record reference.
    /// </summary>
    public const string Code0103 = "01-03";

    /// <summary>
    /// All event messages must be date and time stamped using Universal Coordinated Time
    /// and must contain an event reference.
    /// </summary>
    public const string Code0104 = "01-04";

    /// <summary>
    /// All imported or exported content must be logged.
    /// </summary>
    public const string Code0201 = "02-01";
    
    /// <summary>
    /// All imported or exported content must be subject to content checking.
    /// </summary>
    public const string Code0202 = "02-02";
    
    /// <summary>
    /// All imported or exported content must be scanned against
    /// a security policy (for example to include keyword scanning).
    /// </summary>
    public const string Code0203 = "02-03";
    
    /// <summary>
    /// All malware or suspicious content detected must be logged
    /// and an event generated. The logged data must include the user
    /// who carried out the activity or the process ID if this is
    /// an automated process, the device from where the attempt was
    /// made, the request that was blocked and the malware or content name.
    /// </summary>
    public const string Code0204 = "02-04";

    /// <summary>
    /// If encrypted content cannot be checked at the boundary it
    /// should be discarded or quarantined and the event logged.
    /// </summary>
    public const string Code0207 = "02-07";

    /// <summary>
    /// All blocked file import attempts must be logged. The logged data must
    /// include the user who attempted to import the file or the process ID
    /// if this is an automated process, the device from where the file import
    /// was attempted (IP address and/or host name), the request that was
    /// blocked and the reason why the request was blocked.
    /// </summary>
    public const string Code0210 = "02-10";

    /// <summary>
    /// All allowed file imports must be logged. The logged data must include
    /// the user who imported the file or the process ID if this is an automated process,
    /// the device from where the file import was carried out (IP address and/or host name),
    /// the request that was allowed.
    /// </summary>
    public const string Code0212 = "02-12";

    /// <summary>
    /// File errors, I/O errors and other system errors should be logged
    /// and should be alerted to a systems management system.
    /// </summary>
    public const string Code0406 = "04-06";

    /// <summary>
    /// Any use of an application, network or database administrative facility or accountable 
    /// transaction must be logged and must include the user ID, any domain ID of the user, 
    /// the host on which the action was taken, 
    /// the process, account or session that took the action, 
    /// the user role identifier (such as; administrator, root, power user), 
    /// the application against which the action occurred, 
    /// a description of the action and the outcome of the action
    /// </summary>
    public const string Code0706 = "07-06";
}