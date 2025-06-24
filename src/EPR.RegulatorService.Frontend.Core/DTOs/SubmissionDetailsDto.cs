namespace EPR.RegulatorService.Frontend.Core.DTOs
{
    public class SubmissionDetailsDto
    {
        public bool SubmittedOnTime { get; set; }

        public Guid SubmittedByUserId { get; set; }

        public string AccountRole { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

        public string DeclaredBy {  get; set; }

        public List<FileDetailsDto> Files { get; set; } = [];

        public string Status { get; set; }

        public DateTime DecisionDate { get; set; }

        public DateTime ResubmissionDecisionDate { get; set; }

        public DateTime StatusPendingDate { get; set; }

        public DateTime TimeAndDateOfSubmission { get; set; }

        public string SubmissionPeriod { get; set; }

        public int AccountRoleId { get; set; }

        public string SubmittedBy { get; set; }

        public string ResubmissionStatus { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime TimeAndDateOfResubmission { get; set; }

        public bool IsResubmission { get; set; }

        public string ResubmissionFileId { get; set; }
    }
}
