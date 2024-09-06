using System.Diagnostics.CodeAnalysis;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Core.MockedData;

[ExcludeFromCodeCoverage]
public static class MockedPendingSubmissions
{
    public static List<Submission> GetMockedPendingSubmissions(int begin, int end)
    {
        var pendingSubmissions = new List<Submission>();

        for (int i = begin; i <= end; i++)
        {
            bool isResubmission = (i % 2) == 0;

            pendingSubmissions.Add(new Submission
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                OrganisationType = (i % 2) == 0 ? OrganisationType.DirectProducer : OrganisationType.ComplianceScheme,
                OrganisationReference = i.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6, '0').Insert(3, " "),
                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = DateTime.Now.AddDays(-((i % 179) + 2)),
                SubmissionPeriod = (i % 3) == 0 ? "July to December 2023" : "January to June 2023",
                IsResubmission = isResubmission,
                Decision = "Pending",
                IsResubmissionRequired = false,
                Comments = isResubmission ? "Missing data / wrong submission period" : String.Empty
            });
        }

        return pendingSubmissions;
    }
}