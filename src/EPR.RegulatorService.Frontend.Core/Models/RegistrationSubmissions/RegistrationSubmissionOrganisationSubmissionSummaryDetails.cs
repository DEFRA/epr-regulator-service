namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionOrganisationSubmissionSummaryDetails
{
    public enum FileType { company, brands, partnership }
    public class FileDetails
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileType Type { get; set; }
        public Guid? FileId { get; set; }
        public string FileName { get; set; }
        public string? BlobName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }

    public DateTime? DecisionDate { get; set; }

    public DateTime TimeAndDateOfSubmission { get; set; }

    [JsonPropertyName("resubmissionDate")]
    public DateTime? TimeAndDateOfResubmission { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public bool SubmittedOnTime { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string SubmissionPeriod { get; set; }

    public int? AccountRoleId { get; set; }

    public string Telephone { get; set; }
    public string Email { get; set; }
    public string SubmittedBy { get; set; }
    public string DeclaredBy { get; set; }

    public List<FileDetails> Files { get; set; } = [];

    public bool IsResubmission { get; set; }
}
