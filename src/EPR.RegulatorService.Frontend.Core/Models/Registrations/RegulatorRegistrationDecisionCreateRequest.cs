namespace EPR.RegulatorService.Frontend.Core.Models.Registrations
{
    using Enums;

    public class RegulatorRegistrationDecisionCreateRequest
    {
        public Guid SubmissionId { get; set; }

        public RegulatorDecision Decision { get; set; }

        public string? Comments { get; set; }

        public Guid FileId { get; set; }

        public Guid OrganisationId { get; set; }

        public string OrganisationNumber { get; set; }

        public string OrganisationName { get; set; }

        public string SubmissionPeriod { get; set; }
    }
}