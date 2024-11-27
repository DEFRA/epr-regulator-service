namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EPR.RegulatorService.Frontend.Core.Enums;

    [ExcludeFromCodeCoverage]
    public class OrganisationRegistrationSubmissionSummaryResponse
    {
        public Guid SubmissionId { get; set; }

        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public string OrganisationReference { get; set; }

        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

        public string ApplicationReferenceNumber { get; set; }

        public string RegistrationReferenceNumber { get; set; }

        public DateTime SubmissionDate { get; set; }

        public int RegistrationYear { get; set; }

        public RegistrationSubmissionStatus SubmissionStatus { get; set; }

        public DateTime? StatusPendingDate { get; set; }

        public int NationId { get; set; }
        public DateTime? RegulatorCommentDate { get; internal set; }
        public string? ProducerCommentDate { get; internal set; }
        public Guid? RegulatorUserId { get; internal set; }
    }
}
