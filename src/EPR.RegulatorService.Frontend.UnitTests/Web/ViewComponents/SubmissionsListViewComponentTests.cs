using AutoFixture;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents
{
    using Frontend.Core.Enums;

    [TestClass]
    public class SubmissionsListViewComponentTests : ViewComponentsTestBase
    {
        private Fixture _fixture;
        private List<Submission> _submissions;
        private const string PendingStatus = "Pending";
        private const string ApprovedStatus = "Approved";
        private const string DrinksLtdCompanyName = "DrinksLtd";
        private const string DrinksLtdCompanyReference = "123456";
        private const string JohnDoeFirstName = "John";
        private const string JohnDoeLastName = "Doe";
        private const string JohnDoeEmail = "John.Doe@test.com";
        private const string JohnDoeServiceRole = "Approved Person";
        private const string JohnDoeTelephone = "0011223344";
        private const string SweetsLtdCompanyName = "SweetsLtd";
        private const string SweetsLtdCompanyReference = "987654";
        private const string TonyStarkFirstName = "Tony";
        private const string TonyStarkLastName = "Stark";
        private const string TonyStarkEmail = "tony.stark@test.com";
        private const string TonyStarkServiceRole = "Delegated Person";
        private const string TonyStarkTelephone = "1234567890";

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture();
            _submissions = new List<Submission>
            {
                new()
                {
                    SubmissionId = Guid.NewGuid(),
                    Decision = PendingStatus,
                    SubmittedDate = DateTime.Now,
                    IsResubmission = false,
                    OrganisationId = Guid.NewGuid(),
                    OrganisationName = DrinksLtdCompanyName,
                    OrganisationReference = DrinksLtdCompanyReference,
                    OrganisationType = OrganisationType.DirectProducer,
                    UserId = Guid.NewGuid(),
                    Email = JohnDoeEmail,
                    ServiceRole = JohnDoeServiceRole,
                    FirstName = JohnDoeFirstName,
                    LastName = JohnDoeLastName,
                    Telephone = JohnDoeTelephone
                },
                new()
                {
                    SubmissionId = Guid.NewGuid(),
                    Decision = ApprovedStatus,
                    SubmittedDate = DateTime.Now,
                    IsResubmission = true,
                    OrganisationId = Guid.NewGuid(),
                    OrganisationName = SweetsLtdCompanyName,
                    OrganisationReference = SweetsLtdCompanyReference,
                    OrganisationType = OrganisationType.ComplianceScheme,
                    UserId = Guid.NewGuid(),
                    Email = TonyStarkEmail,
                    ServiceRole = TonyStarkServiceRole,
                    FirstName = TonyStarkFirstName,
                    LastName = TonyStarkLastName,
                    Telephone = TonyStarkTelephone
                }
            };
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_NoFiltersSet()
        {
            // Arrange
            var submissions = _fixture.Build<PaginatedList<Submission>>()
                .With(x => x.Items, _submissions)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Submission>(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<OrganisationType?>(),
                    It.IsAny<string[]>(), It.IsAny<int>()))
                .Returns(Task.FromResult(submissions));

            var viewComponent = new SubmissionsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            // Act
            var result = await viewComponent.InvokeAsync(new SubmissionListRequest
            {
                SearchOrganisationName = string.Empty,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = false,
                IsPendingSubmissionChecked = false,
                IsAcceptedSubmissionChecked = false,
                IsRejectedSubmissionChecked = false
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewData);
            Assert.IsNotNull(result.ViewData.Model);
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();

            var model = result.ViewData.Model as SubmissionsListViewModel;
            model.Should().NotBeNull();
            model.PagedOrganisationSubmissions.Should().BeEquivalentTo(submissions.Items);
            model.PagedOrganisationSubmissions.Count().Should().Be(2);
            model.PaginationNavigationModel.CurrentPage.Should().Be(submissions.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(submissions.TotalPages);
            model.RegulatorSubmissionFiltersModel.SearchOrganisationName.Should().BeEmpty();
            model.RegulatorSubmissionFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsComplianceSchemeChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsPendingSubmissionChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsAcceptedSubmissionChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsRejectedSubmissionChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_ApprovedStatus_ComplianceScheme_OrganisationNameSet()
        {
            // Arrange
            var submissions = _fixture.Build<PaginatedList<Submission>>()
                .With(x => x.Items, _submissions)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            submissions.Items.RemoveAt(0);

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Submission>(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<OrganisationType>(),
                    It.IsAny<string[]>(), It.IsAny<int>()))
                .Returns(Task.FromResult(submissions));

            var viewComponent = new SubmissionsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            var submissionFiltersModel = new SubmissionFiltersModel()
            {
                SearchOrganisationName = SweetsLtdCompanyName,
                IsComplianceSchemeChecked = true,
                IsAcceptedSubmissionChecked = true
            };

            // Act
            var result = await viewComponent.InvokeAsync( new SubmissionListRequest
            {
                SearchOrganisationName = submissionFiltersModel.SearchOrganisationName,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = true,
                IsPendingSubmissionChecked = false,
                IsAcceptedSubmissionChecked = true,
                IsRejectedSubmissionChecked = false,
            });

            var model = result.ViewData.Model as SubmissionsListViewModel;

            // Assert
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            model.Should().NotBeNull();
            model.PagedOrganisationSubmissions.Should().BeEquivalentTo(submissions.Items);
            model.PagedOrganisationSubmissions.Count().Should().Be(1);
            model.PagedOrganisationSubmissions.FirstOrDefault().OrganisationName.Should().Be(SweetsLtdCompanyName);
            model.PagedOrganisationSubmissions.FirstOrDefault().OrganisationType.Should().Be(OrganisationType.ComplianceScheme);
            model.PagedOrganisationSubmissions.FirstOrDefault().Decision.Should().Be(ApprovedStatus);
            model.PaginationNavigationModel.CurrentPage.Should().Be(submissions.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(submissions.TotalPages);
            model.RegulatorSubmissionFiltersModel.SearchOrganisationName.Should().Be(SweetsLtdCompanyName);
            model.RegulatorSubmissionFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsComplianceSchemeChecked.Should().BeTrue();
            model.RegulatorSubmissionFiltersModel.IsPendingSubmissionChecked.Should().BeFalse();
            model.RegulatorSubmissionFiltersModel.IsAcceptedSubmissionChecked.Should().BeTrue();
            model.RegulatorSubmissionFiltersModel.IsRejectedSubmissionChecked.Should().BeFalse();
        }
    }
}