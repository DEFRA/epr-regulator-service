using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Web.Constants;

namespace EPR.RegulatorService.Frontend.UnitTests.TestData
{
    public static class TestSubmission
    {
        public static Submission GetTestSubmission() =>
            new()
            {
                SubmissionId = Guid.NewGuid(),
                SubmittedDate = new DateTime(2023, 03, 03, 03, 03, 33),
                Decision = SubmissionStatus.Pending,
                IsResubmission = false,
                IsResubmissionRequired = false,
                Comments = string.Empty,
                OrganisationId = Guid.NewGuid(),
                OrganisationName = "Test Org Ltd.",
                OrganisationType = "Direct producer",
                OrganisationReference = "123 456",
                Email = "test@abc.com",
                UserId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Telephone = "0123 456 789",
                ServiceRole = "Approved person"
            };
    }
}