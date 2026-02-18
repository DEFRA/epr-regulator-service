namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

[ExcludeFromCodeCoverage]
public class RegistrationSubmissionOrganisationDetailsResponse
{
    public Guid SubmissionId { get; init; }
    public Guid OrganisationId { get; init; }
    public string OrganisationReference { get; init; }
    public string OrganisationName { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionOrganisationType OrganisationType { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationJourneyType RegistrationJourneyType { get; set; }
    public int NationId { get; init; }
    public string NationCode { get; set; }
    public int RelevantYear { get; init; }
    public DateTime SubmissionDate { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus SubmissionStatus { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus? ResubmissionStatus { get; init; }
    public DateTime? StatusPendingDate { get; set; }
    public string? RegulatorComments { get; set; } = string.Empty;
    public string? ProducerComments { get; set; } = string.Empty;
    public string ApplicationReferenceNumber { get; init; } = string.Empty;
    public string? RegistrationReferenceNumber { get; init; } = string.Empty;
    public string CompaniesHouseNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? SubBuildingName { get; set; }
    public string? BuildingNumber { get; set; }
    public string Street { get; set; }
    public string Locality { get; set; }
    public string? DependentLocality { get; set; }
    public string Town { get; set; }
    public string County { get; set; }
    public string Country { get; set; }
    public string Postcode { get; set; }
    public RegistrationSubmissionOrganisationSubmissionSummaryDetails SubmissionDetails { get; set; }
    public DateTime? RegulatorResubmissionDecisionDate { get; set; }
    public DateTime? RegulatorDecisionDate { get; set; }
    public DateTime? ProducerCommentDate { get; set; }
    public Guid? RegulatorUserId { get; set; }
    public bool IsOnlineMarketPlace { get; set; }
    public int NumberOfSubsidiaries { get; set; }
    public int NumberOfOnlineSubsidiaries { get; set; }
    public int NumberOfLateSubsidiaries { get; set; }
    public bool IsLateSubmission { get; set; }
    public string OrganisationSize { get; set; }
    public bool IsComplianceScheme { get; set; }
    public string SubmissionPeriod { get; set; }
    public List<CsoMembershipDetailsDto> CsoMembershipDetails { get; set; }

    public bool IsResubmission { get; set; }
    public string ResubmissionFileId { get; set; }
}