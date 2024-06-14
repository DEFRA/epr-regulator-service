using AutoFixture;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Filters;

[TestClass]
public class AbstractSubmissionFiltersTests
{
    private IQueryable<AbstractSubmission> _abstractSubmissions;
    private Fixture _fixture;

    [TestInitialize]
    public void Initialize()
    {
        _fixture = new Fixture();
        _abstractSubmissions = _fixture
            .Build<AbstractSubmission>()
            .With(x => x.Decision, GetRandomStatus)
            .CreateMany(30)
            .AsQueryable();

        int i = 0;
        string[] submissionPeriods = { "January to June 2023", "July to December 2023", "July to December 2024" };

        foreach (var submission in _abstractSubmissions)
        {
            int periodIndex = i / 10;

            if (periodIndex > submissionPeriods.Length)
            {
                periodIndex = submissionPeriods.Length;
            }

            submission.SubmissionPeriod = submissionPeriods[periodIndex];
            i++;
        }
    }

    public static string GetRandomStatus()
    {
        string[] statuses = { "Pending", "Rejected", "Accepted" };
        Random random = new Random();
        int index = random.Next(statuses.Length);
        return statuses[index];
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(7)]
    [DataRow(12)]
    [DataRow(23)]
    public void FilterByOrganisationNameAndOrganisationReference_OnlyOrgRef_ReturnsMatch(int itemIndex)
    {
        var expectedSubmission = _abstractSubmissions.ToList()[itemIndex];

        var result =
            _abstractSubmissions
                .FilterByOrganisationNameAndOrganisationReference(
                    string.Empty,
                    expectedSubmission.OrganisationReference);

        result.Should().Contain(expectedSubmission);
        result.Count().Should().Be(1);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(7)]
    [DataRow(12)]
    [DataRow(23)]
    public void FilterByOrganisationNameAndOrganisationReference_OnlyOrgName_ReturnsMatch(int itemIndex)
    {
        var expectedSubmission = _abstractSubmissions.ToList()[itemIndex];

        var result =
            _abstractSubmissions
                .FilterByOrganisationNameAndOrganisationReference(
                    expectedSubmission.OrganisationName,
                    String.Empty);

        result.Should().Contain(expectedSubmission);
        result.Count().Should().Be(1);
    }

    [TestMethod]
    [DataRow(1,4)]
    [DataRow(5,2)]
    [DataRow(7,9)]
    [DataRow(12,20)]
    [DataRow(23,13)]
    public void FilterByOrganisationNameAndOrganisationReference_OrgNameAndRef_ReturnsMatch(int itemIndexRef, int itemIndexName)
    {
        var submissionList = _abstractSubmissions.ToList();
        var expectedSubmissionRef = submissionList[itemIndexRef];
        var expectedSubmissionName = submissionList[itemIndexName];

        var result =
            _abstractSubmissions
                .FilterByOrganisationNameAndOrganisationReference(
                    expectedSubmissionName.OrganisationName,
                    expectedSubmissionRef.OrganisationReference);

        result.Should().Contain(expectedSubmissionRef);
        result.Should().Contain(expectedSubmissionName);
        result.Count().Should().Be(2);
    }

    [TestMethod]
    public void FilterByOrganisationNameAndOrganisationReference_NoValues_ReturnsAll()
    {
        var result =
            _abstractSubmissions
                .FilterByOrganisationNameAndOrganisationReference(
                    string.Empty,
                    string.Empty);

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterByOrganisationType_NullType_ReturnsAll()
    {
        var result =
            _abstractSubmissions
                .FilterByOrganisationType(null);

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    [DataRow(OrganisationType.ComplianceScheme)]
    [DataRow(OrganisationType.DirectProducer)]
    public void FilterByOrganisationType_OrgTypeProvided_ReturnsOnlyMatches(OrganisationType orgType)
    {
        var expectedSubmissions =
            _abstractSubmissions.Where(x => x.OrganisationType == orgType);

        var result =
            _abstractSubmissions
                .FilterByOrganisationType(orgType);

        result.Should().BeEquivalentTo(expectedSubmissions);
    }

    [TestMethod]
    public void FilterByStatus_Null_ReturnsAll()
    {
        var result =
            _abstractSubmissions
                .FilterByStatus(null);

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterByStatus_EmptyArray_ReturnsAll()
    {
        var result =
            _abstractSubmissions
                .FilterByStatus(Array.Empty<string>());

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    [DataRow(new[]{"Pending"})]
    [DataRow(new[]{"Accepted"})]
    [DataRow(new[]{"Rejected"})]
    [DataRow(new[]{"Pending", "Accepted"})]
    [DataRow(new[]{"Pending", "Rejected"})]
    [DataRow(new[]{"Accepted", "Rejected"})]
    [DataRow(new[]{"Pending", "Accepted", "Rejected"})]
    public void FilterByStatus_StatusesPassed_ReturnsOnlyMatches(string[] statusArray)
    {
        var expectedSubmissions =
            _abstractSubmissions.Where(x => statusArray.Contains(x.Decision));

        var result =
            _abstractSubmissions
                .FilterByStatus(statusArray);

        result.Should().BeEquivalentTo(expectedSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionYears_Null_ReturnsAll()
    {
        var result = _abstractSubmissions.FilterBySubmissionYears(null);

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionYears_EmptyArray_ReturnsAll()
    {
        var result = _abstractSubmissions.FilterBySubmissionYears(Array.Empty<int>());

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionYears_SubmissionYearsPassed_ReturnsOnlyMatches()
    {
        var submissionYears = new[] { 2023 };

        var expectedSubmissions = _abstractSubmissions.Where(
            x => submissionYears.Contains(int.Parse(x.SubmissionPeriod.Substring(x.SubmissionPeriod.Length - 4))));

        var result = _abstractSubmissions.FilterBySubmissionYears(submissionYears);

        result.Should().BeEquivalentTo(expectedSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionPeriods_Null_ReturnsAll()
    {
        var result = _abstractSubmissions.FilterBySubmissionPeriods(null);

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionPeriods_EmptyArray_ReturnsAll()
    {
        var result = _abstractSubmissions.FilterBySubmissionPeriods(Array.Empty<string>());

        result.Should().BeEquivalentTo(_abstractSubmissions);
    }

    [TestMethod]
    public void FilterBySubmissionPeriods_SubmissionPeriodsPassed_ReturnsOnlyMatches()
    {
        var submissionPeriods = new[] { "January to June 2023" };

        var expectedSubmissions = _abstractSubmissions.Where(x => submissionPeriods.Contains(x.SubmissionPeriod));

        var result = _abstractSubmissions.FilterBySubmissionPeriods(submissionPeriods);

        result.Should().BeEquivalentTo(expectedSubmissions);
    }
}