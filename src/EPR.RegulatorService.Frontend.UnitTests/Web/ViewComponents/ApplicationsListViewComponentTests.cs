using AutoFixture;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents
{
    [TestClass]
    public class ApplicationsListViewComponentTests : ViewComponentsTestBase
    {
        private const string DrinksLtdCompanyName = "DrinksLtd";
        private const string SweetsLtdCompanyName = "SweetsLtd";
        private Fixture _fixture;
        private List<OrganisationApplications> _organisationApplications;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture();
            _organisationApplications = new List<OrganisationApplications>
            {
                new()
                {
                    OrganisationName = DrinksLtdCompanyName,
                    OrganisationId = Guid.NewGuid(),
                    Enrolments = new PendingEnrollments()
                    {
                        HasApprovedPending = false, HasDelegatePending = false
                    }
                },
                new()
                {
                    OrganisationName = SweetsLtdCompanyName,
                    OrganisationId = Guid.NewGuid(),
                    Enrolments = new PendingEnrollments()
                    {
                        HasApprovedPending = false, HasDelegatePending = false
                    }
                }
            };
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_NoFiltersSet()
        {
            // Arrange
            var userApplications = _fixture.Build<PaginatedList<OrganisationApplications>>()
                .With(x => x.Items, _organisationApplications)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            _facadeServiceMock
                .Setup(x => x.GetUserApplicationsByOrganisation(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
                .Returns(Task.FromResult(userApplications));

            var viewComponent = new ApplicationsListViewComponent(_facadeServiceMock.Object);

            // Act
            var result = await viewComponent.InvokeAsync(null, false, false);
            

            // Assert
            Assert.IsNotNull(result);
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            Assert.IsNotNull(result.ViewData);
            Assert.IsNotNull(result.ViewData.Model);

            var model = result.ViewData.Model as ApplicationsListViewModel;
            Assert.IsNotNull(model);

            Assert.IsNotNull(model.PagedOrganisationApplications);
            model.PagedOrganisationApplications.Should().BeEquivalentTo(userApplications.Items);
            model.PagedOrganisationApplications.Count().Should().Be(2);
            model.PaginationNavigationModel.CurrentPage.Should().Be(userApplications.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(userApplications.TotalPages);
            model.RegulatorApplicationFiltersModel.SearchOrganisationName.Should().BeNull();
            model.RegulatorApplicationFiltersModel.IsApprovedUserTypeChecked.Should().BeFalse();
            model.RegulatorApplicationFiltersModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_Organisation_Is_DrinksLtd_And_Delegated_Persons_Only()
        {
            // Arrange
            _organisationApplications.FirstOrDefault(x => x.OrganisationName == DrinksLtdCompanyName)
                .Enrolments
                .HasDelegatePending = true;
            var userApplications = _fixture.Build<PaginatedList<OrganisationApplications>>()
                .With(x => x.Items, _organisationApplications)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 1)
                .Create();

            var expectedResult = userApplications;
            expectedResult.Items.RemoveAt(1);
            
            _facadeServiceMock
                .Setup(x => x.GetUserApplicationsByOrganisation(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
                .Returns(Task.FromResult(expectedResult));

            var viewComponent = new ApplicationsListViewComponent(_facadeServiceMock.Object);

            // Act
            var result = await viewComponent.InvokeAsync(DrinksLtdCompanyName, false, true);            

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewData);
            Assert.IsNotNull(result.ViewData.Model);
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();

            var model = result.ViewData.Model as ApplicationsListViewModel;
            Assert.IsNotNull(model);

            Assert.IsNotNull(model.PagedOrganisationApplications);
            model.PagedOrganisationApplications.Should().BeEquivalentTo(userApplications.Items);
            model.PagedOrganisationApplications.Count().Should().Be(1);
            model.PaginationNavigationModel.CurrentPage.Should().Be(userApplications.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(userApplications.TotalPages);
            model.RegulatorApplicationFiltersModel.SearchOrganisationName.Should().Be(DrinksLtdCompanyName);
            model.RegulatorApplicationFiltersModel.IsApprovedUserTypeChecked.Should().BeFalse();
            model.RegulatorApplicationFiltersModel.IsDelegatedUserTypeChecked.Should().BeTrue();
        }
    }   
}
