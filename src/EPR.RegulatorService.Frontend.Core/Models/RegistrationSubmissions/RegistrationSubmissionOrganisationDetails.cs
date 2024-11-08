namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

/// <summary>
/// The Frontend representation of the Results from the Facade for
/// the registrations/get-organisations endpoint
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("{OrganisationName}, {OrganisationReference}, {RegistrationYear}, {RegistrationStatus},{OrganisationType}")]
public sealed class RegistrationSubmissionOrganisationDetails : IEquatable<RegistrationSubmissionOrganisationDetails?>
{
    public Guid SubmissionId { get; set; }
    public Guid OrganisationID { get; set; }
    public string OrganisationReference { get; set; }
    public string OrganisationName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

    public int NationID { get; set; }
    public string RegistrationYear { get; set; }
    public DateTime RegistrationDateTime { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus RegistrationStatus { get; set; }

    public string? RegulatorComments { get; set; } = string.Empty;
    public string? ProducerComments { get; set; } = string.Empty;
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

    public RegistrationSubmissionOrganisationSubmissionSummaryDetails SubmissionDetails { get; set; }
    public RegistrationSubmissionsOrganisationPaymentDetails PaymentDetails { get; set; }

    public override bool Equals(object? obj) => Equals(obj as RegistrationSubmissionOrganisationDetails);
    public bool Equals(RegistrationSubmissionOrganisationDetails? other) => other is not null && OrganisationID.Equals(other.OrganisationID);
    public override int GetHashCode() => HashCode.Combine(OrganisationID);

    public static bool operator ==(RegistrationSubmissionOrganisationDetails? left, RegistrationSubmissionOrganisationDetails? right) => EqualityComparer<RegistrationSubmissionOrganisationDetails>.Default.Equals(left, right);
    public static bool operator !=(RegistrationSubmissionOrganisationDetails? left, RegistrationSubmissionOrganisationDetails? right) => !(left == right);
}

