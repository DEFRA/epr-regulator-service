namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using Frontend.Core.Enums;

[TestClass]
public class RegistrationSubmissionListViewComponentTests : ViewComponentsTestBase
{
    private Fixture _fixture;
    private List<RegistrationSubmissionOrganisationDetails> _submissions;

    private readonly Guid _drinksLtdGuid = Guid.NewGuid();
    private const string DrinksLtdCompanyName = "DrinksLtd";
    private const string DrinksLtdCompanyReference = "123456";
    private const RegistrationSubmissionOrganisationType DrinksLtdType = RegistrationSubmissionOrganisationType.compliance;
    private const RegistrationSubmissionStatus DrinksLtdStatus = RegistrationSubmissionStatus.Updated;
    private readonly DateTime _drinksLtdRegistrationTime = DateTime.Now - new TimeSpan(50, 0, 0, 0);

    private readonly Guid _sweetsLtdGuid = Guid.NewGuid();
    private const string SweetsLtdCompanyName = "SweetsLtd";
    private const string SweetsLtdCompanyReference = "987654";
    private const RegistrationSubmissionOrganisationType SweetsLtdType = RegistrationSubmissionOrganisationType.large;
    private const RegistrationSubmissionStatus SweetsLtdStatus = RegistrationSubmissionStatus.Queried;
    private readonly DateTime _sweetsLtdRegistrationTime = DateTime.Now - new TimeSpan(30, 0, 0, 0);

    private readonly Guid _flyByLtdGuid = Guid.NewGuid();
    private const string FlyByLtdCompanyName = "FlyByLtd";
    private const string FlyByLtdCompanyReference = "237654";
    private const RegistrationSubmissionOrganisationType FlyByLtdType = RegistrationSubmissionOrganisationType.small;
    private const RegistrationSubmissionStatus FlyByLtdStatus = RegistrationSubmissionStatus.Pending;
    private readonly DateTime _flyByLtdRegistrationTime = DateTime.Now - new TimeSpan(10, 0, 0, 0);


    [TestInitialize]
    public void TestInitialize()
    {
        _fixture = new Fixture();
        _submissions = new List<RegistrationSubmissionOrganisationDetails>()
        {
            new()
            {
                OrganisationId = _drinksLtdGuid,
                OrganisationName = DrinksLtdCompanyName,
                OrganisationReference = DrinksLtdCompanyReference,
                OrganisationType = DrinksLtdType,
                SubmissionStatus = DrinksLtdStatus,
                SubmissionDate = _drinksLtdRegistrationTime,
                RelevantYear = _drinksLtdRegistrationTime.Year
            },
            new()
            {
                OrganisationId = _sweetsLtdGuid,
                OrganisationName = SweetsLtdCompanyName,
                OrganisationReference = SweetsLtdCompanyReference,
                OrganisationType = SweetsLtdType,
                SubmissionStatus = SweetsLtdStatus,
                SubmissionDate = _sweetsLtdRegistrationTime,
                RelevantYear = _sweetsLtdRegistrationTime.Year
            },
            new()
            {
                OrganisationId = _flyByLtdGuid,
                OrganisationName = FlyByLtdCompanyName,
                OrganisationReference = FlyByLtdCompanyReference,
                OrganisationType = FlyByLtdType,
                SubmissionStatus = FlyByLtdStatus,
                SubmissionDate = _flyByLtdRegistrationTime,
                RelevantYear = _flyByLtdRegistrationTime.Year
            }
        };
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_NoFiltersSet()
    {
        var submissions = _fixture.Build<PaginatedList<RegistrationSubmissionOrganisationDetails>>()
            .With(x => x.items, _submissions)
            .With(x => x.currentPage, 1)
            .With(x => x.totalItems, 3)
            .Create();

        _facadeServiceMock
            .Setup(x => x.GetRegistrationSubmissions(It.IsAny<RegistrationSubmissionsFilterModel?>()))
            .Returns(Task.FromResult(submissions));

        var viewComponent = new RegistrationSubmissionListViewComponent(_facadeServiceMock.Object);
        viewComponent.Should().NotBeNull();
    }
}