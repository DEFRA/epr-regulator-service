namespace EPR.RegulatorService.Frontend.UnitTests.MockedData;

using Frontend.Core.Enums;
using Frontend.Core.MockedData.Filters;
using Frontend.Core.Models.Submissions;

//this test class is created in response to sonarqube flagging lower code coverage
[TestClass]
public class MockedSubmissionFiltersTests
{
    private const string OrganisationName = "DirectProducer_Accepted_111111";
    private const string OrganisationReference = "123 456";
    private const string Accepted = "Accepted";
    private const string Rejected = "Rejected";
    private const string Pending = "Pending";

    private static List<Submission> _allSubmissions = GenerateOrganisationSubmissions();

    [TestMethod]
    public async Task Test_Filter_By_Organisation_Name_And_Organisation_Reference()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(OrganisationName, OrganisationReference).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow(OrganisationType.DirectProducer)]
    [DataRow(OrganisationType.ComplianceScheme)]
    public async Task Test_Filter_By_Organisation_Type(OrganisationType organisationType)
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByOrganisationType(organisationType).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task Test_Filter_By_Submission_Status_Accepted()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByStatus(new []{ Accepted }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Submission_Status_Rejected()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByStatus(new []{ Rejected }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Submission_Status_Pending()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByStatus(new []{ Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Submission_Status_Accepted_And_Pending()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByStatus(new []{ Accepted, Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(4);
    }

    [TestMethod]
    public async Task Test_Filter_By_Submission_Status_Rejected_And_Pending()
    {
        var results = _allSubmissions
            .AsQueryable()
            .FilterByStatus(new []{ Rejected, Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(4);
    }

    private static List<Submission> GenerateOrganisationSubmissions()
    {
        var allItems = new List<Submission>
        {
            new()
            {
                OrganisationName = "DirectProducer_Accepted_111111",
                OrganisationReference = "111 111",
                OrganisationType = OrganisationType.DirectProducer,
                Decision = Accepted
            },
            new()
            {
                OrganisationName = "ComplianceScheme_Accepted_123456",
                OrganisationReference = "123 456",
                OrganisationType = OrganisationType.ComplianceScheme,
                Decision = Accepted
            },
            new()
            {
                OrganisationName = "DirectProducer_Rejected_222222",
                OrganisationReference = "222 222",
                OrganisationType = OrganisationType.DirectProducer,
                Decision = Rejected
            },
            new()
            {
                OrganisationName = "ComplianceScheme_Rejected_333333",
                OrganisationReference = "333 333",
                OrganisationType = OrganisationType.ComplianceScheme,
                Decision = Rejected
            },
            new()
            {
                OrganisationName = "DirectProducer_Pending_444444",
                OrganisationReference = "444 444",
                OrganisationType = OrganisationType.DirectProducer,
                Decision = Pending
            },
            new()
            {
                OrganisationName = "ComplianceScheme_Pending_555555",
                OrganisationReference = "555 555",
                OrganisationType = OrganisationType.ComplianceScheme,
                Decision = Pending
            }
        };

        return allItems;
    }
}