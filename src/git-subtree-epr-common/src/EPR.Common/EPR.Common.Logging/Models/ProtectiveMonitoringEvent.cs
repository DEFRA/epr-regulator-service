namespace EPR.Common.Logging.Models;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Metadata for protective monitoring event that can be sent to the Service Bus Queue.
/// </summary>
/// <param name="SessionId">The session ID used for event correlation.</param>
/// <param name="Component">The EPR component name, e.g. Account Microservice.</param>
/// <param name="PmcCode">The code from DEFRA Protective Monitoring Specification. Use <see cref="T:EPR.Common.Logging.Constants.PmcCode"> for predefined values.</param>
/// <param name="Priority">The priority of the event. Use <see cref="T:EPR.Common.Logging.Constants.Priorities"> for predefined values.</param>
/// <param name="TransactionCode">The transaction code of the event. Use <see cref="T:EPR.Common.Logging.Constants.TransactionCodes"> for predefined values.</param>
/// <param name="Message">A free text describing the event.</param>
/// <param name="AdditionalInfo">A free text supporting details.</param>
[ExcludeFromCodeCoverage]
public record ProtectiveMonitoringEvent(
    [Required] Guid SessionId,
    [Required] string Component,
    [Required] string PmcCode,
    [Required] int Priority,
    [Required] string TransactionCode,
    string Message,
    string AdditionalInfo);