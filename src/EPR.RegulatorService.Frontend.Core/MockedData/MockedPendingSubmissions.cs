using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.MockedData;

[ExcludeFromCodeCoverage]
public static class MockedPendingSubmissions
{
    public static List<Submission> GetMockedPendingSubmissions(int begin, int end)
    {
        var pendingSubmissions = new List<Submission>();
        var random = new Random();

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = random.Next(2) == 0;

            pendingSubmissions.Add(new Submission
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                OrganisationType = (i % 2) == 0 ? "Direct producer" : "Compliance scheme",
                OrganisationReference = i.ToString().PadLeft(6, '0').Insert(3, " "),
                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = DateTime.Now.AddDays(-random.Next(2, 180)),
                SubmissionPeriod = "July to December 2023",
                IsResubmission = isResubmission,
                Decision = "Pending",
                IsResubmissionRequired = false,
                Comments = isResubmission ? "Missing data / wrong submission period" : String.Empty
            });
        }

        return pendingSubmissions;
    }
}