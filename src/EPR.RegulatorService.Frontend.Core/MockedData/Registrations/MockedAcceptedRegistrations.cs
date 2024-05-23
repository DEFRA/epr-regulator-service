namespace EPR.RegulatorService.Frontend.Core.MockedData.Registrations;

using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using System.Diagnostics.CodeAnalysis;
using Enums;
using System.Security.Cryptography;

[ExcludeFromCodeCoverage]
public static class MockedAcceptedRegistrations
{
    public static List<Registration> GetMockedAcceptedRegistrations(int begin, int end)
    {
        var acceptedRegistrations = new List<Registration>();
        var random = RandomNumberGenerator.GetInt32(2);

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = random == 0;

            acceptedRegistrations.Add(new Registration
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                OrganisationType = (i % 2) == 0 ? OrganisationType.DirectProducer : OrganisationType.ComplianceScheme,
                OrganisationReference = i.ToString().PadLeft(6,'0').Insert(3, " "),
                CompaniesHouseNumber = RandomNumberGenerator.GetInt32(1000000000).ToString(),

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
                RegistrationDate = DateTime.Now.AddDays(-RandomNumberGenerator.GetInt32(2,180)),
                IsResubmission = isResubmission,
                Decision = "Accepted",
                RejectionComments = String.Empty,
                PreviousRejectionComments = isResubmission ? "Rejected because reasons" : string.Empty,
                OrganisationDetailsFileId = Guid.NewGuid(),
                OrganisationDetailsFileName = "OrgDetails.csv",
                PartnershipFileId = Guid.NewGuid(),
                PartnershipFileName = "PartnerDetails.csv",
                BrandsFileId = Guid.NewGuid(),
                BrandsFileName = "BrandDetails.csv"
            });
        }

        return acceptedRegistrations;
    }
}
