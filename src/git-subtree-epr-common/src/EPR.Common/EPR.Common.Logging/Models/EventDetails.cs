namespace EPR.Common.Logging.Models;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal record EventDetails(
    [Required] string TransactionCode,
    string Message,
    string AdditionalInfo);