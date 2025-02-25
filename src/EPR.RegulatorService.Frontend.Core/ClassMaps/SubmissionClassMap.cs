namespace EPR.RegulatorService.Frontend.Core.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;
using EPR.RegulatorService.Frontend.Core.Models;

[ExcludeFromCodeCoverage]
public class SubmissionClassMap : ClassMap<SubmissionCsvModel>
{
    public SubmissionClassMap()
    {
        Map(m => m.Organisation).Name("organisation");
        Map(m => m.OrganisationId).Name("organisation_id");
        Map(m => m.SubmissionDate).Name("submission_date_and_time");
        Map(m => m.SubmissionPeriod).Name("submission_period");
        Map(m => m.Status).Name("status");
    }
}
