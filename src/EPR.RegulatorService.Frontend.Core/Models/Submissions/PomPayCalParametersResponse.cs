using System;
using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PomPayCalParametersResponse
{
    public bool? IsResubmission { get; set; }

    public DateTime? ResubmissionDate { get; set; }

    public int? MemberCount { get; set; }

    public string? Reference { get; set; }

    [JsonPropertyName(name: "referenceNotAvailable")]
    public bool ReferenceNotAvailable { get; set; }

    [JsonPropertyName(name: "referenceFieldNotAvailable")]
    public bool ReferenceFieldNotAvailable { get; set; }
}