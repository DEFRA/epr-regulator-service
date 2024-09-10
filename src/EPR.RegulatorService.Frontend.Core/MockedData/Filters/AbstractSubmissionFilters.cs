using EPR.RegulatorService.Frontend.Core.Enums;
using  EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.MockedData.Filters;

public static class AbstractSubmissionFilters
{
    public static IQueryable<AbstractSubmission> FilterByOrganisationNameAndOrganisationReference(this IQueryable<AbstractSubmission> query,
        string organisationName, string organisationReference)
    {
        bool organisationNameSet = !string.IsNullOrWhiteSpace(organisationName);
        bool organisationReferenceSet = !string.IsNullOrWhiteSpace(organisationReference);

        if (organisationNameSet && organisationReferenceSet)
        {
            query = query.Where(o =>
                (o.OrganisationName.Contains(organisationName, StringComparison.OrdinalIgnoreCase))
                || (o.OrganisationReference.Contains(organisationReference, StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            if (organisationNameSet)
            {
                query = query.Where(o =>
                    o.OrganisationName.Contains(organisationName, StringComparison.OrdinalIgnoreCase));
            }

            if (organisationReferenceSet)
            {
                query = query.Where(o =>
                    o.OrganisationReference.Contains(organisationReference, StringComparison.OrdinalIgnoreCase));
            }
        }

        return query;
    }

    public static IQueryable<AbstractSubmission> FilterByOrganisationType(this IQueryable<AbstractSubmission> query,
        OrganisationType? organisationType)
    {
        if (organisationType != null)
        {
            query = query.Where(o => o.OrganisationType == organisationType);
        }

        return query;
    }

    public static IQueryable<AbstractSubmission> FilterByStatus(this IQueryable<AbstractSubmission> query,
        string[] registrationStatuses)
    {
        if (registrationStatuses?.Length > 0)
        {
            query = query.Where(o => registrationStatuses.Any(o.Decision.Contains));
        }

        return query;
    }

    public static IQueryable<AbstractSubmission> FilterBySubmissionYears(this IQueryable<AbstractSubmission> query, int[] submissionYears)
    {
        if (submissionYears?.Length >0)
        {
            query = query.Where(x => submissionYears.Contains(int.Parse(x.SubmissionPeriod.Substring(x.SubmissionPeriod.Length - 4))));
        }

        return query;
    }

    public static IQueryable<AbstractSubmission> FilterBySubmissionPeriods(this IQueryable<AbstractSubmission> query, string[] submissionPeriods)
    {
        if (submissionPeriods?.Length > 0)
        {
            query = query.Where(x => submissionPeriods.Contains(x.SubmissionPeriod));
        }

        return query;
    }
}