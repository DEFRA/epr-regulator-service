using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.MockedData;

[ExcludeFromCodeCoverage]
[SuppressMessage("Major Code Smell", "S2245:Random should not be used for security-sensitive applications", Justification = "Used only for generating test data")]
public static class MockedAcceptedSubmissions
{
    public static List<Submission> GetMockedAcceptedSubmissions(int begin, int end)
    {
        var acceptedSubmissions = new List<Submission>();
        var random = new Random();

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = random.Next(2) == 0;

            acceptedSubmissions.Add(new Submission
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                OrganisationType = (i % 2) == 0 ? OrganisationType.DirectProducer : OrganisationType.ComplianceScheme,
                OrganisationReference = i.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6, '0').Insert(3, " "),
                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test User",
                LastName = "Test User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = DateTime.Now.AddDays(-random.Next(2, 180)),
                IsResubmission = isResubmission,
                Decision = "Accepted",
                IsResubmissionRequired = false,
                Comments = String.Empty
            });
        }

        return acceptedSubmissions;
    }
}