namespace EPR.RegulatorService.Frontend.Core.MockedData.Filters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc.ApplicationParts;

public static class RegistrationSubmissionsFilters
{
    public static IQueryable<RegistrationSubmissionOrganisationDetails> Filter(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, RegistrationSubmissionsFilterModel filters) => queryable
                        .FilterByOrganisationName(filters.OrganisationName)
                        .FilterByOrganisationRef(filters.OrganisationRef)
                        .FilterByOrganisationType(filters.OrganisationType)
                        .FilterBySubmissionStatus(filters.SubmissionStatus)
                        .FilterByRelevantYear(filters.RelevantYear);

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationName(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? organisationName)
    {
        if (!String.IsNullOrEmpty(organisationName))
        {
            queryable = from q in queryable
                        where q.OrganisationName.Contains(organisationName, StringComparison.OrdinalIgnoreCase)
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationRef(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, string? organisationRef)
    {
        if (!String.IsNullOrEmpty(organisationRef))
        {
            queryable = from q in queryable
                        where q.OrganisationReference.Contains(organisationRef, StringComparison.OrdinalIgnoreCase)
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByOrganisationType(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, RegistrationSubmissionOrganisationType? organisationType)
    {
        if (organisationType.HasValue && organisationType.Value != RegistrationSubmissionOrganisationType.none)
        {
            queryable = from q in queryable
                        where q.OrganisationType.Equals(organisationType.Value)
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterBySubmissionStatus(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, RegistrationSubmissionStatus? submissionStatus)
    {
        if (submissionStatus.HasValue && submissionStatus.Value != RegistrationSubmissionStatus.none)
        {
            queryable = from q in queryable
                        where q.RegistrationStatus.Equals(submissionStatus.Value)
                        select q;
        }

        return queryable;
    }

    public static IQueryable<RegistrationSubmissionOrganisationDetails> FilterByRelevantYear(this IQueryable<RegistrationSubmissionOrganisationDetails> queryable, int? relevantYear)
    {
        if (relevantYear.HasValue)
        {
            queryable = from q in queryable
                        where q.RegistrationYear.Equals(relevantYear.Value)
                        select q;
        }

        return queryable;
    }
}