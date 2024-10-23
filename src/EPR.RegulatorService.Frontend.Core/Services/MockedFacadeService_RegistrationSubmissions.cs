namespace EPR.RegulatorService.Frontend.Core.Services;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public partial class MockedFacadeService : IFacadeService
{
    public static List<RegistrationSubmissionOrganisationDetails> GenerateRegistrationSubmissionDataCollection()
    {
        List<RegistrationSubmissionOrganisationDetails> objRet = new();

        foreach (string line in RegistrationSubmissionTestData.TSVData)
        {
            string[] fields = line.Split('\t');

            var dateTime = DateTime.Parse(fields[8], new CultureInfo("en-GB"));
            objRet.Add(new RegistrationSubmissionOrganisationDetails
            {
                OrganisationReference = fields[0],
                OrganisationName = fields[1],
                OrganisationType = Enum.Parse<RegistrationSubmissionOrganisationType>(fields[2]),
                RegistrationStatus = Enum.Parse<RegistrationSubmissionStatus>(fields[3]),
                ApplicationReferenceNumber = fields[4],
                RegistrationReferenceNumber = fields[5],
                RegistrationDateTime = dateTime,
                RegistrationYear = dateTime.Year.ToString(),
                CompaniesHouseNumber = fields[9],
                BuildingName = fields[10],
                SubBuildingName = fields[11],
                BuildingNumber = fields[12],
                Street = fields[13],
                Locality = fields[14],
                DependentLocality = fields[15],
                Town = fields[16],
                County = fields[17],
                Country = fields[18],
                Postcode = fields[19],
                OrganisationID = Guid.Parse(fields[22]),
                NationID = int.Parse(fields[23], CultureInfo.InvariantCulture),
            });
        }

        return objRet;
    }

    public Tuple<int, List<RegistrationSubmissionOrganisationDetails>> FilterAndOrderRegistrations(List<RegistrationSubmissionOrganisationDetails> items, RegistrationSubmissionsFilterModel filters)
    {
        var rawItems = items.AsQueryable();

        var filteredItems = rawItems.Filter(filters);

        int totalItems = filteredItems.Count();

        var sortedItems = filteredItems
                .OrderBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.refused)
                .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.granted)
                .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.cancelled)
                .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.updated)
                .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.queried)
                .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.pending)
                .ThenBy(x => x.RegistrationDateTime)
                .Skip((filters.Page.Value - 1) * _config.PageSize)
                .Take(filters.PageSize ?? _config.PageSize)
                .ToList();

        return Tuple.Create(totalItems, sortedItems);
    }
}
