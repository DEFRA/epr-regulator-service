using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.MockedData.Filters;

public static class RegistrationFilters
{
    public static IQueryable<Registration> FilterByOrganisationNameAndOrganisationReference(this IQueryable<Registration> query,
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

    public static IQueryable<Registration> FilterByOrganisationType(this IQueryable<Registration> query,
        OrganisationType? organisationType)
    {
        if (organisationType != null)
        {
            query = query.Where(o => o.OrganisationType == organisationType);
        }

        return query;
    }

    public static IQueryable<Registration> FilterByRegistrationStatus(this IQueryable<Registration> query,
        string[] registrationStatuses)
    {
        if (registrationStatuses.Length != 0)
        {
            query = query.Where(o => registrationStatuses.Any(o.Decision.Contains));
        }

        return query;
    }
}