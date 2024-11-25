namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionOrganisationSubmissionSummaryDetails
{
    public enum FileType { company, brands, partnership }
    public class FileDetails
    {
        public FileType Type { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RegistrationSubmissionStatus Status { get; set; }

    public DateTime DecisionDate { get; set; }

    public DateTime TimeAndDateOfSubmission { get; set; }
    public bool SubmittedOnTime { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string SubmissionPeriod { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceRole AccountRole { get; set; }

    public string Telephone { get; set; }
    public string Email { get; set; }
    public string DeclaredBy { get; set; }

    public List<FileDetails> Files { get; set; } = [];
}
