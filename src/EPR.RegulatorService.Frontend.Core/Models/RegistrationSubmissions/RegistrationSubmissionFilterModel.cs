namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

/// <summary>
/// Used to filter results from the Facade API for RegistrationSubmissions
/// </summary>
[ExcludeFromCodeCoverage]
public class RegistrationSubmissionsFilterModel
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
                                    .Where(part => !articlesToExclude.Contains(part.ToLowerInvariant())));
                _organisationName = filteredName;
            }
        }
    }

    public string? OrganisationReference { get; set; } = string.Empty;

    public string? OrganisationType { get; set; }

    public string? Statuses { get; set; }

    public string? ResubmissionStatuses { get; set; }

    public string? RelevantYears { get; set; }

    public string? ApplicationReferenceNumber { get; set; }

    public string? RegistrationReferenceNumber { get; set; }

    public int? PageNumber { get; set; } = 1;

    public int? PageSize { get; set; } = 20;

    public int NationId { get; set; }

    public bool Show2026RelevantYearFilter { get; set; }

    // TODO: If we use Serilog for object de-structuring, we won't need to do this (https://eaflood.atlassian.net/browse/SMAL-252)
    public Dictionary<string, object?> ToDictionary() =>
        new()
        {
            { $"Filters_{nameof(OrganisationName)}", OrganisationName },
            { $"Filters_{nameof(OrganisationReference)}", OrganisationReference },
            { $"Filters_{nameof(OrganisationType)}", OrganisationType },
            { $"Filters_{nameof(Statuses)}", Statuses },
            { $"Filters_{nameof(ResubmissionStatuses)}", ResubmissionStatuses },
            { $"Filters_{nameof(RelevantYears)}", RelevantYears },
            { $"Filters_{nameof(ApplicationReferenceNumber)}", ApplicationReferenceNumber },
            { $"Filters_{nameof(RegistrationReferenceNumber)}", RegistrationReferenceNumber },
            { $"Filters_{nameof(PageNumber)}", PageNumber },
            { $"Filters_{nameof(PageSize)}", PageSize },
            { $"Filters_{nameof(NationId)}", NationId },
            { $"Filters_{nameof(Show2026RelevantYearFilter)}", Show2026RelevantYearFilter }
        };
}
