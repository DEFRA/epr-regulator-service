namespace EPR.Common.Logging.Config;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class LoggingApiConfig
{
    public static string SectionName => "LoggingApi";

    [Required]
    public string BaseUrl { get; init; }
}