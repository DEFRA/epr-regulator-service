using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Filters;

[TestClass]
public class RegistrationSubmissionFiltersTests
{
    private IQueryable<RegistrationSubmissionOrganisationDetails> _abstractRegistrations;
    private Fixture _fixture;
    private static readonly Random _random = new();

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
        _abstractRegistrations = _fixture
            .Build<RegistrationSubmissionOrganisationDetails>()
            .With(x => x.SubmissionStatus, GetRandomStatus)
            .With(x => x.OrganisationType, GetRandomOrgType)
            .CreateMany(50)
            .AsQueryable();
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(12)]
    [DataRow(19)]
    [DataRow(25)]
    [DataRow(29)]
    public void FilterByOrganisationName_ReturnsOnlyByOrgName(int byIndex)
    {
        var expectedResult = _abstractRegistrations.ToArray()[byIndex];

        var result = _abstractRegistrations.FilterByOrganisationName(expectedResult.OrganisationName);
        result.Should().Contain(expectedResult);
        result.Count().Should().Be(1);
    }

    [TestMethod]
    public void FilterByOrganisationName_WithNoValue_ReturnsAll()
    {
        var result = _abstractRegistrations.FilterByOrganisationName("");
        result.Count().Should().Be(_abstractRegistrations.Count());
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(8)]
    [DataRow(14)]
    [DataRow(20)]
    [DataRow(25)]
    public void FilterByOrganisationName_ReturnsOnlyByOrgRef(int byIndex)
    {
        var expectedResult = _abstractRegistrations.ToArray()[byIndex];

        var result = _abstractRegistrations.FilterByOrganisationRef(expectedResult.OrganisationReference);
        result.Should().Contain(expectedResult);
        result.Count().Should().Be(1);
    }

    [TestMethod]
    public void FilterByOrganisationRef_WithNoValue_ReturnsAll()
    {
        var result = _abstractRegistrations.FilterByOrganisationRef("");
        result.Count().Should().Be(_abstractRegistrations.Count());
    }

    [TestMethod]
    [DataRow(RegistrationSubmissionOrganisationType.large)]
    [DataRow(RegistrationSubmissionOrganisationType.compliance)]
    [DataRow(RegistrationSubmissionOrganisationType.small)]
    public void FilterByOrganisationName_ReturnsOnlyByOrgType(RegistrationSubmissionOrganisationType byType)
    {
        var expectedResult = _abstractRegistrations.Where(x => x.OrganisationType == byType);

        var result = _abstractRegistrations.FilterByOrganisationType(byType.ToString());
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public void FilterByOrganisationType_WithNoValue_ReturnsAll()
    {
        var result = _abstractRegistrations.FilterByOrganisationType(null);
        result.Count().Should().Be(_abstractRegistrations.Count());

        result = _abstractRegistrations.FilterByOrganisationType(RegistrationSubmissionOrganisationType.none.ToString());
        result.Count().Should().Be(_abstractRegistrations.Count());
    }

    [TestMethod]
    [DataRow(RegistrationSubmissionStatus.Updated)]
    [DataRow(RegistrationSubmissionStatus.Queried)]
    [DataRow(RegistrationSubmissionStatus.Cancelled)]
    [DataRow(RegistrationSubmissionStatus.Granted)]
    [DataRow(RegistrationSubmissionStatus.Refused)]
    public void FilterByOrganisationName_ReturnsOnlyBySubmissionStatus(RegistrationSubmissionStatus byStatus)
    {
        var expectedResult = _abstractRegistrations.Where(x=>x.SubmissionStatus == byStatus);

        var result = _abstractRegistrations.FilterBySubmissionStatus(byStatus.ToString());
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public void FilterByRegistrationStatus_WithNoValue_ReturnsAll()
    {
        var result = _abstractRegistrations.FilterBySubmissionStatus (null);
        result.Count().Should().Be(_abstractRegistrations.Count());

        result = _abstractRegistrations.FilterBySubmissionStatus(RegistrationSubmissionStatus.None.ToString());
        result.Count().Should().Be(_abstractRegistrations.Count());
    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(15)]
    [DataRow(19)]
    [DataRow(23)]
    [DataRow(44)]
    public void FilterByRegistrationYear_ReturnsOnlyThatYear(int byIndex)
    {
        var expectedYear = _abstractRegistrations.ToArray()[byIndex].RelevantYear;
        var expectedResult = _abstractRegistrations.Where(x=>x.RelevantYear == expectedYear);

        var result = _abstractRegistrations.FilterByRelevantYear(expectedYear.ToString());
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(15)]
    [DataRow(19)]
    [DataRow(23)]
    [DataRow(44)]
    public void FilterByNameSizeStatusAndYear_ReturnsTheCorrectSet(int byIndex)
    {
        var item = _abstractRegistrations.ToArray()[byIndex];
        var expectedItems = new List<RegistrationSubmissionOrganisationDetails>
        {
            item
        };

        string expectedName = item.OrganisationName[3..6];
        var expectedSize = item.OrganisationType;
        var expectedStatus = item.SubmissionStatus;
        var expectedYear = item.RelevantYear;

        var filter = new RegistrationSubmissionsFilterModel
        {
            OrganisationName = expectedName,
            OrganisationType = expectedSize.ToString(),
            Statuses = expectedStatus.ToString(),
            RelevantYears = expectedYear.ToString()
        };

        var result = _abstractRegistrations.Filter(filter).ToList();
        result.Should().BeEquivalentTo(expectedItems);
    }

    private static RegistrationSubmissionStatus GetRandomStatus()
    {
        RegistrationSubmissionStatus[] enums = [RegistrationSubmissionStatus.Granted, RegistrationSubmissionStatus.Refused, RegistrationSubmissionStatus.Queried, RegistrationSubmissionStatus.Updated, RegistrationSubmissionStatus.Cancelled, RegistrationSubmissionStatus.Pending];
        int index = _random.Next(enums.Length);
        return enums[index];
    }

    private static RegistrationSubmissionOrganisationType GetRandomOrgType()
    {
        RegistrationSubmissionOrganisationType[] enums = [RegistrationSubmissionOrganisationType.compliance, RegistrationSubmissionOrganisationType.small, RegistrationSubmissionOrganisationType.large];
        int index = _random.Next(enums.Length);
        return enums[index];
    }

}
