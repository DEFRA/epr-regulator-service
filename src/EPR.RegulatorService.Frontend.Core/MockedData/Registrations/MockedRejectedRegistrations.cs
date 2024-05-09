// BEGIN-NOSCAN
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.MockedData.Registrations;

[ExcludeFromCodeCoverage]
public static class MockedRejectedRegistrations
{
    public static List<Registration> GetMockedRejectedRegistrations(int begin, int end)
    {
        var rejectedRegistrations = new List<Registration>();
        var random = RandomNumberGenerator.GetInt32(2);

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = random == 0;

            rejectedRegistrations.Add(new Registration
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                OrganisationType = (i % 2) == 0 ? OrganisationType.DirectProducer : OrganisationType.ComplianceScheme,
                OrganisationReference = i.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6,'0').Insert(3, " "),
                CompaniesHouseNumber = RandomNumberGenerator.GetInt32(1000000000).ToString(System.Globalization.CultureInfo.InvariantCulture),

                BuildingName = "Building name",
                SubBuildingName = "Sub-building name",
                BuildingNumber = "Building number",
                Street = "Street",
                Locality = "Locality",
                DependantLocality = "Dependant locality",
                Town = "Town",
                County = "County",
                Country = "Country",
                PostCode = "PostCode",

                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Sally",
                LastName = "Smith",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",

                SubmissionId = Guid.NewGuid(),
                SubmissionPeriod = "January 2023 to June 2023",
                RegistrationDate = DateTime.Now.AddDays(-RandomNumberGenerator.GetInt32(2, 180)),
                IsResubmission = isResubmission,
                Decision = "Rejected",
                RejectionComments = "Missing data / wrong submission period",

                OrganisationDetailsFileId = Guid.NewGuid(),
                OrganisationDetailsFileName = "OrgDetails.csv",
                PartnershipDetailsFileId = Guid.NewGuid(),
                PartnershipDetailsFileName = "PartnerDetails.csv",
                BrandDetailsFileId = Guid.NewGuid(),
                BrandDetailsFileName = "BrandDetails.csv"
            });
        }

        return rejectedRegistrations;
    }
}
// END-NOSCAN