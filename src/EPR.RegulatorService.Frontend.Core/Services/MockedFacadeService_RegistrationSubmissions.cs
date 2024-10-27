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
                OrganisationReference = fields[0][..10],
                OrganisationName = fields[1],
                OrganisationType = Enum.Parse<RegistrationSubmissionOrganisationType>(fields[2]),
                RegistrationStatus = Enum.Parse<RegistrationSubmissionStatus>(fields[3]),
                ApplicationReferenceNumber = fields[4],
                RegistrationReferenceNumber = fields[5],
                RegistrationDateTime = dateTime,
                RegistrationYear = dateTime.Year.ToString(CultureInfo.InvariantCulture),
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
                RegulatorComments = fields[20],
                ProducerComments = fields[21],
            });
        }

        return objRet;
    }

    public Tuple<int, List<RegistrationSubmissionOrganisationDetails>> FilterAndOrderRegistrations(List<RegistrationSubmissionOrganisationDetails> items, RegistrationSubmissionsFilterModel filters)
    {
        var rawItems = items.AsQueryable();

        var filteredItems = rawItems.Filter(filters);

        int totalItems = filteredItems.Count();

        if (filters.Page > (int)Math.Ceiling(totalItems / (double)_config.PageSize))
        {
            filters.Page = (int)Math.Ceiling(totalItems / (double)_config.PageSize);
        }

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

    public static RegistrationSubmissionOrganisationSubmissionSummaryDetails GenerateRandomSubmissionData(RegistrationSubmissionStatus registrationStatus)
    {
        var random = new Random();

        string[] sampleNames = ["Alice", "Bob", "Charlie", "Diana", "Edward"];
        var sampleRoles = Enum.GetValues(typeof(ServiceRole));
        var generateRandomPhoneNumber  = (Random random) => $"{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}";


        return new RegistrationSubmissionOrganisationSubmissionSummaryDetails
        {
            Status = registrationStatus,
            DecisionDate = DateTime.Now.AddDays(-random.Next(1, 100)),
            TimeAndDateOfSubmission = DateTime.Now.AddDays(-random.Next(1, 100)),
            SubmittedOnTime = random.Next(2) == 0,
            SubmittedBy = sampleNames[random.Next(sampleNames.Length)],
            AccountRole = (ServiceRole)sampleRoles.GetValue(random.Next(sampleRoles.Length)),
            Telephone = generateRandomPhoneNumber(random),
            Email = $"{sampleNames[random.Next(sampleNames.Length)].ToLower(CultureInfo.CurrentCulture)}@example.com",
            DeclaredBy = sampleNames[random.Next(sampleNames.Length)],
            Files = GenerateRandomFiles(random.Next(1, 5)) // Generate 1 to 5 random files
        };
    }

    private static List<RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails> GenerateRandomFiles(int count)
    {
        var files = new List<RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails>();
        for (int i = 0; i < count; i++)
        {
            files.Add(new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
            {
                Label = $"Document {i + 1}",
                FileName = $"file_{i + 1}.pdf",
                DownloadUrl = $"https://example.com/download/file_{i + 1}.pdf"
            });
        }
        return files;
    }

    private static RegistrationSubmissionsOrganisationPaymentDetails GeneratePaymentDetails()
    {
        var random = new Random();

        var generateRandomDecimal = (int min, int max) => Math.Round((decimal)(random.NextDouble() * (max - min) + min), 2);

        return new RegistrationSubmissionsOrganisationPaymentDetails()
        {
            ApplicationProcessingFee = generateRandomDecimal(1000, 6000),
            OnlineMarketplaceFee = generateRandomDecimal(1000, 5000),
            PreviousPaymentsReceived = generateRandomDecimal(1000, 10000000),
            SubsidiaryFee = generateRandomDecimal(1000, 10000)
        };
    }

}