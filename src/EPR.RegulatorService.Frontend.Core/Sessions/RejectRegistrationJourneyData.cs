using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

[ExcludeFromCodeCoverage]
public class RejectRegistrationJourneyData
{
    public string? OrganisationName { get; set; }
    public Guid SubmissionId { get; set; }
    public string? SubmittedBy { get; set; }
}
