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
    private string? _organisationName = string.Empty;

    public string? OrganisationName
    {
        get => _organisationName;
        set
        {
            if (null == value)
            {
                _organisationName = null;
            }
            else
            {
                string[] articlesToExclude = ["-", "the", "a", "an", "and", "&", "or", "of", "for", "in", "'n", "+"];
                string filteredName = String.Join(" ", value
                                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Where(part => !articlesToExclude.Contains(part.ToLower())));

                _organisationName = filteredName;
            }
        }
    }

    public string? OrganisationRef { get; set; } = string.Empty;

    public string? OrganisationType { get; set; }
    public string? SubmissionStatus { get; set; }

    public string? RelevantYear { get; set; }

    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}
