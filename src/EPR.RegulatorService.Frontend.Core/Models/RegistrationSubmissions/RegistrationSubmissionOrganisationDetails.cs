namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The Frontend representation of the Results from the Facade for
/// the registrations/get-organisations endpoint
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("{OrganisationName}, {OrganisationReference}, {RelevantYear}, {SubmissionStatus},{OrganisationType}")]
public sealed class RegistrationSubmissionOrganisationDetails : IEquatable<RegistrationSubmissionOrganisationDetails?>
{
    public Guid SubmissionId { get; set; }
    public Guid OrganisationId
    { get; set; }
    public string OrganisationReference { get; set; }
    public string OrganisationName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

    public int NationId { get; set; }

    public string NationCode { get; set; }
    public int RelevantYear { get; set; }
    public DateTime SubmissionDate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus SubmissionStatus { get; set; }
    public DateTime? StatusPendingDate { get; set; }

    public int NumberOfSubsidiaries { get; set; }
    public int NumberOfOnlineSubsidiaries { get; set; }
    public bool IsOnlineMarketPlace { get; set; }

    public string? RegulatorComments { get; set; } = string.Empty;
    public string? ProducerComments { get; set; } = string.Empty;
    public DateTime? RegulatorDecisionDate { get; set; }
    public DateTime? ProducerCommentDate { get; set; }
    public string ApplicationReferenceNumber { get; set; } = String.Empty;
    public string? RegistrationReferenceNumber { get; set; } = String.Empty;
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

    public string? RejectReason { get; set; } = string.Empty;
    public string? CancellationReason { get; set; } = string.Empty;

    public RegistrationSubmissionOrganisationSubmissionSummaryDetails SubmissionDetails { get; set; }
    public RegistrationSubmissionsOrganisationPaymentDetails PaymentDetails { get; set; }
    public string? RegulatorDescisionDate { get; set; }

    public bool IsLateSubmission { get; internal set; }
    public bool? IsComplianceScheme { get; set; }
    public string OrganisationSize { get; set; }

    public string SubmissionPeriod { get; internal set; }

    public override bool Equals(object? obj) => Equals(obj as RegistrationSubmissionOrganisationDetails);
    public bool Equals(RegistrationSubmissionOrganisationDetails? other) => other is not null && OrganisationId.Equals(other.OrganisationId);
    public override int GetHashCode() => HashCode.Combine(OrganisationId);

    public static bool operator ==(RegistrationSubmissionOrganisationDetails? left, RegistrationSubmissionOrganisationDetails? right) => EqualityComparer<RegistrationSubmissionOrganisationDetails>.Default.Equals(left, right);
    public static bool operator !=(RegistrationSubmissionOrganisationDetails? left, RegistrationSubmissionOrganisationDetails? right) => !(left == right);

    public static implicit operator RegistrationSubmissionOrganisationDetails(OrganisationRegistrationSubmissionSummaryResponse response) => response is null
            ? null
            : new RegistrationSubmissionOrganisationDetails
            {
                SubmissionId = response.SubmissionId,
                OrganisationId = response.OrganisationId,
                OrganisationName = response.OrganisationName,
                OrganisationReference = response.OrganisationReference,
                OrganisationType = response.OrganisationType,
                ApplicationReferenceNumber = response.ApplicationReferenceNumber,
                RegistrationReferenceNumber = response.RegistrationReferenceNumber,
                SubmissionDate = response.SubmissionDate,
                NationId = response.NationId,
                RelevantYear = response.RegistrationYear,
                SubmissionStatus = response.SubmissionStatus,
                StatusPendingDate = response.StatusPendingDate,
                ProducerCommentDate = response.ProducerCommentDate,
                RegulatorDecisionDate = response.RegulatorCommentDate
            };

    public static implicit operator RegistrationSubmissionOrganisationDetails(RegistrationSubmissionOrganisationDetailsResponse response) => response is null
        ? null
        : new RegistrationSubmissionOrganisationDetails
        {
            SubmissionId = response.SubmissionId,
            OrganisationId = response.OrganisationId,
            OrganisationReference = response.OrganisationReference,
            OrganisationName = response.OrganisationName,
            OrganisationType = response.OrganisationType,
            NationId = response.NationId,
            NationCode = response.NationCode,
            RelevantYear = response.RelevantYear,
            SubmissionDate = response.SubmissionDate,
            SubmissionStatus = response.SubmissionStatus,
            StatusPendingDate = response.StatusPendingDate,
            RegulatorComments = response.RegulatorComments,
            ProducerComments = response.ProducerComments,
            ApplicationReferenceNumber = response.ApplicationReferenceNumber,
            RegistrationReferenceNumber = response.RegistrationReferenceNumber,
            CompaniesHouseNumber = response.CompaniesHouseNumber,
            BuildingName = response.BuildingName,
            SubBuildingName = response.SubBuildingName,
            BuildingNumber = response.BuildingNumber,
            Street = response.Street,
            Locality = response.Locality,
            DependentLocality = response.DependentLocality,
            Town = response.Town,
            County = response.County,
            Country = response.Country,
            Postcode = response.Postcode,
            IsComplianceScheme = response.IsComplianceScheme,
            OrganisationSize = response.OrganisationSize,
            NumberOfSubsidiaries = response.NumberOfSubsidiaries,
            NumberOfOnlineSubsidiaries = response.NumberOfSubsidiaries,
            SubmissionDetails = response.SubmissionDetails,
            IsLateSubmission = response.IsLateSubmission,
            RegulatorDecisionDate = response.RegulatorDecisionDate,
            ProducerCommentDate = response.ProducerCommentDate,
            IsOnlineMarketPlace = response.IsOnlineMarketPlace,
        };
}
        

