namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using EPR.RegulatorService.Frontend.Core.Enums;

    [ExcludeFromCodeCoverage]
    public class OrganisationRegistrationSubmissionSummaryResponse
    {
        public Guid SubmissionId { get; set; }

        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public string OrganisationReference { get; set; }

        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }
        public RegistrationJourneyType RegistrationJourneyType { get; set; }

        public string ApplicationReferenceNumber { get; set; }

        public string RegistrationReferenceNumber { get; set; }

        public DateTime SubmissionDate { get; set; }

        public int RegistrationYear { get; set; }

        public RegistrationSubmissionStatus SubmissionStatus { get; set; }

        public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }

        public DateTime? ResubmissionDecisionDate { get; set; }

        public DateTime? StatusPendingDate { get; set; }

        public int NationId { get; set; }

        public DateTime? RegulatorCommentDate { get; set; }

        public DateTime? ProducerCommentDate { get; set; }

        public Guid? RegulatorUserId { get; set; }

        public bool IsResubmission { get; set; }

        public DateTime? ResubmissionDate { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public DateTime? RegulatorDecisionDate { get; set; }
        public string ResubmissionFileId { get; internal set; }
    }
}
