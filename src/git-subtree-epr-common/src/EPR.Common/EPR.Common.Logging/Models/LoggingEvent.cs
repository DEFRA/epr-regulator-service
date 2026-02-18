namespace EPR.Common.Logging.Models;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal record LoggingEvent(
    [Required] Guid UserId,
    [Required] Guid SessionId,
    [Required] DateTime DateTime,
    [Required] string Component,
    [Required] string PmcCode,
    [Required] int Priority,
    [Required] EventDetails Details,
    string? Ip);
