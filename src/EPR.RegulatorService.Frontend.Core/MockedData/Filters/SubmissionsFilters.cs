namespace EPR.RegulatorService.Frontend.Core.MockedData.Filters;

using Models.Submissions;

public static class SubmissionsFilters
{
    public static IQueryable<Submission> FilterByOrganisationNameAndOrganisationReference(this IQueryable<Submission> query,
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

    public static IQueryable<Submission> FilterByOrganisationType(this IQueryable<Submission> query,
        string organisationType)
    {
        if (!string.IsNullOrWhiteSpace(organisationType))
        {
            query = query.Where(o => o.OrganisationType.Contains(organisationType, StringComparison.OrdinalIgnoreCase));
        }
        return query;
    }

    public static IQueryable<Submission> FilterBySubmissionStatus(this IQueryable<Submission> query,
        string[] submissionStatuses)
    {
        if (submissionStatuses.Length != 0)
        {
            query = query.Where(o => submissionStatuses.Any(o.Decision.Contains));
        }
        return query;
    }
}