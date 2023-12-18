using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.MockedData;

[ExcludeFromCodeCoverage]
public static class MockedRejectedSubmissions
{
    public static List<Submission> GetMockedRejectedSubmissions(int begin, int end)
    {
        var rejectedSubmissions = new List<Submission>();
        var random = new Random();

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = random.Next(2) == 0;

            rejectedSubmissions.Add(new Submission
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
                IsResubmission = isResubmission,
                Decision = "Rejected",
                IsResubmissionRequired = true,
                Comments = "Missing data / wrong submission period"
            });
        }

        return rejectedSubmissions;
    }
}