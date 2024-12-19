namespace EPR.RegulatorService.Frontend.Core.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionCsvModel
{
    public string Organisation {  get; set; }

    public string OrganisationId { get; set; }

    public string SubmissionDate { get; set; }

    public string SubmissionPeriod { get; set; }

    public string Status { get; set; }
}
