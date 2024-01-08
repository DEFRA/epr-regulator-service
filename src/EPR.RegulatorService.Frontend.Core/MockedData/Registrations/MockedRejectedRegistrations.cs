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
                OrganisationReference = i.ToString().PadLeft(6,'0').Insert(3, " "),

                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test User",
                LastName = "Test User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",

                RegistrationId = Guid.NewGuid(),
                RegistrationDate = DateTime.Now.AddDays(-RandomNumberGenerator.GetInt32(2, 180)),
                IsResubmission = isResubmission,
                Decision = "Rejected",
                RejectionComments = "Missing data / wrong submission period"
            });
        }

        return rejectedRegistrations;
    }
}
// END-NOSCAN