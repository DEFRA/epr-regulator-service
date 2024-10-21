namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Enums;

[ExcludeFromCodeCoverage]
/// <summary>
/// Used to filter results from the Facade API for RegistrationSubmissions
/// </summary>
public  class RegistrationSubmissionsFilterModel
{
    public string? OrganisationName { get; set; } = string.Empty;
    public string? OrganisationRef { get; set; } = string.Empty;
    public RegistrationSubmissionOrganisationType? OrganisationType { get; set; } = RegistrationSubmissionOrganisationType.none;
    public RegistrationSubmissionStatus? SubmissionStatus { get; set; } = RegistrationSubmissionStatus.none;
    public int? RelevantYear { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}
