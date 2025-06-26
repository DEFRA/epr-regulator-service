namespace EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions
{
    using System.Text.Json.Serialization;

    using EPR.RegulatorService.Frontend.Core.Enums;

    public class SubmissionDetailsDto
    {
        public bool SubmittedOnTime { get; set; }

        public Guid SubmittedByUserId { get; set; }

        public string AccountRole { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

        public string DeclaredBy { get; set; }

        public List<FileDetailsDto> Files { get; set; } = [];

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegistrationSubmissionStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }

        public DateTime? DecisionDate { get; set; }

        public DateTime? ResubmissionDecisionDate { get; set; }

        [JsonPropertyName("statusPendingDate")]
        public DateTime? StatusPendingDate { get; set; }

        public DateTime TimeAndDateOfSubmission { get; set; }

        [JsonPropertyName("resubmissionDate")]
        public DateTime? TimeAndDateOfResubmission { get; set; }

        public string SubmissionPeriod { get; set; }

        public int AccountRoleId { get; set; }

        public string SubmittedBy { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool IsResubmission { get; set; }

        public string ResubmissionFileId { get; set; }
    }
}
