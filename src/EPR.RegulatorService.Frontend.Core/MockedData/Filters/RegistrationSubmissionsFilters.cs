namespace EPR.RegulatorService.Frontend.Core.MockedData.Filters;

using System;
using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public static class RegistrationSubmissionsFilters
{
    public static IQueryable<RegistrationSubmissionOrganisationDetails> Filter(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable,
                                                                                RegistrationSubmissionsFilterModel filters) => queryable
                        .FilterByOrganisationName(filters.OrganisationName)
                        .FilterByOrganisationRef(filters.OrganisationReference)
                        .FilterByOrganisationType(filters.OrganisationType)
                        .FilterBySubmissionStatus(filters.Statuses)
                        .FilterByReSubmissionStatus(filters.ResubmissionStatuses)
                        .FilterByRelevantYear(filters.RelevantYears);

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationName(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? organisationName)
    {
        if (!string.IsNullOrEmpty(organisationName))
        {
            string[] nameParts = organisationName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var completeQueryable = from q in queryable
                                    where Array.TrueForAll(nameParts, part => q.OrganisationName.Contains(part, StringComparison.OrdinalIgnoreCase))
                                    select q;

            queryable = completeQueryable.Any()
                ? completeQueryable
                : (from q in queryable
                   where nameParts.Any(part => q.OrganisationName.Contains(part, StringComparison.OrdinalIgnoreCase))
                   select q);
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationRef(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? organisationRef)
    {
        if (!string.IsNullOrEmpty(organisationRef))
        {
            string[] nameParts = organisationRef.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            queryable = from q in queryable
                        where nameParts.Any(part => q.OrganisationReference.Contains(part, StringComparison.OrdinalIgnoreCase))
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationType(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? organisationType)
    {
        if (!string.IsNullOrEmpty(organisationType) && organisationType != "none")
        {
            queryable = from q in queryable
                        where organisationType.Contains(q.OrganisationType.ToString())
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterBySubmissionStatus(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? submissionStatus)
    {
        if (!string.IsNullOrEmpty(submissionStatus) && submissionStatus.ToLower() != "none")
        {
            queryable = from q in queryable
                        where submissionStatus.Contains(q.SubmissionStatus.ToString())
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByReSubmissionStatus(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? resubmissionStatus)
    {
        if (!string.IsNullOrEmpty(resubmissionStatus) && resubmissionStatus.ToLower() != "none")
        {
            queryable = from q in queryable
                        where resubmissionStatus.Contains(q.ResubmissionStatus.ToString())
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByRelevantYear(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? relevantYear)
    {
        if (!string.IsNullOrEmpty(relevantYear))
        {
            queryable = from q in queryable
                        where relevantYear.Contains(q.RelevantYear.ToString())
                        select q;
        }

        return queryable;
    }
}