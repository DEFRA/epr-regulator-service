using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Web.Constants;

namespace EPR.RegulatorService.Frontend.UnitTests.TestData
{
    using Frontend.Core.Enums;

    public static class TestSubmission
    {
        public static Submission GetTestSubmission() =>
            new()
            {
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = new DateTime(2023, 03, 03, 03, 03, 33, DateTimeKind.Unspecified),
                Decision = SubmissionStatus.Pending,
                IsResubmission = false,
                IsResubmissionRequired = false,
                Comments = string.Empty,
                OrganisationId = Guid.NewGuid(),
                OrganisationName = "Test Org Ltd.",
                OrganisationType = OrganisationType.DirectProducer,
                OrganisationReference = "123 456",
                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",
                PomBlobName = "A blob name",
                PomFileName = "A Pom File.csv"
            };

        public static Submission GetTestSubmission(string prefix) =>
            new()
            {
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = new DateTime(2023, 03, 03, 03, 03, 33, DateTimeKind.Unspecified),
                Decision = SubmissionStatus.Pending,
                IsResubmission = false,
                IsResubmissionRequired = false,
                Comments = string.Empty,
                OrganisationId = Guid.NewGuid(),
                OrganisationName = $"{prefix} Test Org Ltd.",
                OrganisationType = OrganisationType.DirectProducer,
                OrganisationReference = "123 456",
                Email = $"{prefix}test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = $"{prefix} Test",
                LastName = $"{prefix} User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person",
                PomBlobName = "A blob name",
                PomFileName = $"{prefix} A Pom File.csv"
            };
    }
}