using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;

namespace EPR.RegulatorService.Frontend.UnitTests.MockedData;

[Ignore("This will be removed")]
[TestClass]
public class MockedRegistrationFiltersTests
{
    private const string OrganisationName = "DirectProducer_Accepted_111111";
    private const string OrganisationReference = "123 456";
    private const string Accepted = "Accepted";
    private const string Rejected = "Rejected";
    private const string Pending = "Pending";

    private static readonly List<Registration> _allRegistrations = GenerateOrganisationRegistrations();

    [TestMethod]
    public async Task Test_Filter_By_Organisation_Name_And_Organisation_Reference()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(OrganisationName, OrganisationReference).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow("DirectProducer", 3)]
    [DataRow("Pending", 2)]
    [DataRow("2", 2)]
    public async Task Test_Filter_By_Organisation_Name(string organisationName, int expectedCount)
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(organisationName, null);

        Assert.IsNotNull(results);
        results.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow("111", 1)]
    [DataRow("222 222", 1)]
    [DataRow("2", 2)]
    [DataRow("5", 2)]
    [DataRow("6", 1)]
    public async Task Test_Filter_By_Organisation_Reference(string organisationReference, int expectedCount)
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(null, organisationReference);

        Assert.IsNotNull(results);
        results.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow(OrganisationType.DirectProducer)]
    [DataRow(OrganisationType.ComplianceScheme)]
    public async Task Test_Filter_By_Organisation_Type(OrganisationType organisationType)
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByOrganisationType(organisationType).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task Test_Filter_By_Registration_Status_Accepted()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByStatus(new []{ Accepted }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Registration_Status_Rejected()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByStatus(new []{ Rejected }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Registration_Status_Pending()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByStatus(new []{ Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Test_Filter_By_Registration_Status_Accepted_And_Pending()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByStatus(new []{ Accepted, Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(4);
    }

    [TestMethod]
    public async Task Test_Filter_By_Registration_Status_Rejected_And_Pending()
    {
        var results = _allRegistrations
            .AsQueryable()
            .FilterByStatus(new []{ Rejected, Pending }).ToList();

        Assert.IsNotNull(results);
        results.Should().HaveCount(4);
    }

    private static List<Registration> GenerateOrganisationRegistrations()
    {
        var allItems = new List<Registration>
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