using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public class ProducerFeeWaiveDetails
{
    public FeeTypeToWaive FeeType { get; set; }

    public ReasonForWaiver WaiveReason { get; set; }

    public int WaivedAmount { get; set; }

    public string? Comments { get; set; }

    public string WaivedBy { get; set; } = string.Empty;

    public DateTime WaivedDate { get; set; }

    public string? ApplicationReferenceNumber { get; set; }
}
