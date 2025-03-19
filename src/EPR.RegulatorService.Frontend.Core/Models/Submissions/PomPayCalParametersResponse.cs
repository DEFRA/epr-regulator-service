using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PomPayCalParametersResponse
{
    [JsonPropertyName(name: "resubmissionDate")]
    public DateTime? ResubmissionDate { get; set; }

    [JsonPropertyName(name: "memberCount")]
    public int? MemberCount { get; set; }

    [JsonPropertyName(name: "reference")]
    public string? Reference { get; set; }

    [JsonPropertyName(name: "nationCode")]
    public string? NationCode { get; set; }

    [JsonPropertyName(name: "referenceNotAvailable")]
    public bool ReferenceNotAvailable { get; set; }

    [JsonPropertyName(name: "referenceFieldNotAvailable")]
    public bool ReferenceFieldNotAvailable { get; set; }
}