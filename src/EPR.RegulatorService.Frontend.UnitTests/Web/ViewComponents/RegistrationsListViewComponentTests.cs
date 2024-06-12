using AutoFixture;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents
{
    using Frontend.Core.Enums;
    using Frontend.Web.Constants;

    [TestClass]
    public class RegistrationsListViewComponentTests : ViewComponentsTestBase
    {
        private Fixture _fixture;
        private List<Registration> _registrations;
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
            _registrations = new List<Registration>
            {
                new()
                {
                    SubmissionId = Guid.NewGuid(),
                    Decision = PendingStatus,
                    RegistrationDate = DateTime.Now,
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
                    RegistrationDate = DateTime.Now,
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
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<OrganisationType?>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent = new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            // Act
            var result = await viewComponent.InvokeAsync(new RegistrationListRequest
            {
                SearchOrganisationName = string.Empty,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = false,
                IsPendingRegistrationChecked = false,
                IsAcceptedRegistrationChecked = false,
                IsRejectedRegistrationChecked = false
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewData);
            Assert.IsNotNull(result.ViewData.Model);
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();

            var model = result.ViewData.Model as RegistrationsListViewModel;
            model.Should().NotBeNull();
            model.PagedOrganisationRegistrations.Should().BeEquivalentTo(registrations.Items);
            model.PagedOrganisationRegistrations.Count().Should().Be(2);
            model.PaginationNavigationModel.CurrentPage.Should().Be(registrations.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(registrations.TotalPages);
            model.RegulatorRegistrationFiltersModel.SearchOrganisationName.Should().BeEmpty();
            model.RegulatorRegistrationFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsComplianceSchemeChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsPendingRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsRejectedRegistrationChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_ApprovedStatus_ComplianceScheme_OrganisationNameSet()
        {
            // Arrange
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            registrations.Items.RemoveAt(0);

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<OrganisationType>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent = new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            var registrationFiltersModel = new RegistrationFiltersModel()
            {
                SearchOrganisationName = SweetsLtdCompanyName,
                IsComplianceSchemeChecked = true,
                IsAcceptedRegistrationChecked = true
            };

            // Act
            var result = await viewComponent.InvokeAsync(new RegistrationListRequest
            {
                SearchOrganisationName = registrationFiltersModel.SearchOrganisationName,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = true,
                IsPendingRegistrationChecked = false,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = false,
            });

            var model = result.ViewData.Model as RegistrationsListViewModel;

            // Assert
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            model.Should().NotBeNull();
            model.PagedOrganisationRegistrations.Should().BeEquivalentTo(registrations.Items);
            model.PagedOrganisationRegistrations.Count().Should().Be(1);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationName.Should().Be(SweetsLtdCompanyName);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationType.Should().Be(OrganisationType.ComplianceScheme);
            model.PagedOrganisationRegistrations.FirstOrDefault().Decision.Should().Be(ApprovedStatus);
            model.PaginationNavigationModel.CurrentPage.Should().Be(registrations.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(registrations.TotalPages);
            model.RegulatorRegistrationFiltersModel.SearchOrganisationName.Should().Be(SweetsLtdCompanyName);
            model.RegulatorRegistrationFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsComplianceSchemeChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsPendingRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsRejectedRegistrationChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task InvokeAsync_InvalidPageNumber_SendsToErrorPage()
        {
            // Arrange
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    null,
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent =
                new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);
            _httpContextMock.Setup(x => x.Response.Redirect(PagePath.PageNotFound));

            SetViewComponentContext(PagePath.PageNotFound, viewComponent, null);

            // Act
            await viewComponent.InvokeAsync(new RegistrationListRequest()
            {
                SearchOrganisationName = string.Empty,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = false,
                IsPendingRegistrationChecked = false,
                IsAcceptedRegistrationChecked = false,
                IsRejectedRegistrationChecked = false,
                PageNumber = 9
            });

            // Assert
            _httpContextMock.Verify(x => x.Response.Redirect(PagePath.PageNotFoundPath), Times.Once);
        }

        [TestMethod]
        public async Task
            InvokeAsync_ReturnsCorrectViewAndModel_Where_PendingStatus_ComplianceScheme_OrganisationNameSet()
        {
            // Arrange
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            registrations.Items.RemoveAt(0);

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<OrganisationType>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent =
                new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            var registrationFiltersModel = new RegistrationFiltersModel()
            {
                SearchOrganisationName = SweetsLtdCompanyName,
                IsComplianceSchemeChecked = true,
                IsAcceptedRegistrationChecked = true
            };

            // Act
            var result = await viewComponent.InvokeAsync(new RegistrationListRequest
            {
                SearchOrganisationName = registrationFiltersModel.SearchOrganisationName,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = true,
                IsPendingRegistrationChecked = true,
                IsAcceptedRegistrationChecked = false,
                IsRejectedRegistrationChecked = false,
            });

            var model = result.ViewData.Model as RegistrationsListViewModel;

            // Assert
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            model.Should().NotBeNull();
            model.PagedOrganisationRegistrations.Should().BeEquivalentTo(registrations.Items);
            model.PagedOrganisationRegistrations.Count().Should().Be(1);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationName.Should().Be(SweetsLtdCompanyName);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationType.Should()
                .Be(OrganisationType.ComplianceScheme);
            model.PagedOrganisationRegistrations.FirstOrDefault().Decision.Should().Be(ApprovedStatus);
            model.PaginationNavigationModel.CurrentPage.Should().Be(registrations.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(registrations.TotalPages);
            model.RegulatorRegistrationFiltersModel.SearchOrganisationName.Should().Be(SweetsLtdCompanyName);
            model.RegulatorRegistrationFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsComplianceSchemeChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsPendingRegistrationChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsRejectedRegistrationChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task
            InvokeAsync_ReturnsCorrectViewAndModel_Where_RejectedStatus_ComplianceScheme_OrganisationNameSet()
        {
            // Arrange
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            registrations.Items.RemoveAt(0);

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<OrganisationType>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent =
                new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            var registrationFiltersModel = new RegistrationFiltersModel()
            {
                SearchOrganisationName = SweetsLtdCompanyName,
                IsComplianceSchemeChecked = true,
                IsAcceptedRegistrationChecked = true
            };

            // Act
            var result = await viewComponent.InvokeAsync(new RegistrationListRequest
            {
                SearchOrganisationName = registrationFiltersModel.SearchOrganisationName,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = false,
                IsComplianceSchemeChecked = true,
                IsPendingRegistrationChecked = false,
                IsAcceptedRegistrationChecked = false,
                IsRejectedRegistrationChecked = true,
            });

            var model = result.ViewData.Model as RegistrationsListViewModel;

            // Assert
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            model.Should().NotBeNull();
            model.PagedOrganisationRegistrations.Should().BeEquivalentTo(registrations.Items);
            model.PagedOrganisationRegistrations.Count().Should().Be(1);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationName.Should().Be(SweetsLtdCompanyName);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationType.Should()
                .Be(OrganisationType.ComplianceScheme);
            model.PagedOrganisationRegistrations.FirstOrDefault().Decision.Should().Be(ApprovedStatus);
            model.PaginationNavigationModel.CurrentPage.Should().Be(registrations.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(registrations.TotalPages);
            model.RegulatorRegistrationFiltersModel.SearchOrganisationName.Should().Be(SweetsLtdCompanyName);
            model.RegulatorRegistrationFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsComplianceSchemeChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsPendingRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsRejectedRegistrationChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsCorrectViewAndModel_Where_Accepted_DirectProducer_OrganisationNameSet()
        {
            // Arrange
            var registrations = _fixture.Build<PaginatedList<Registration>>()
                .With(x => x.Items, _registrations)
                .With(x => x.CurrentPage, 1)
                .With(x => x.TotalItems, 2)
                .Create();

            registrations.Items.RemoveAt(0);

            _facadeServiceMock
                .Setup(x => x.GetOrganisationSubmissions<Registration>(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<OrganisationType>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()))
                .Returns(Task.FromResult(registrations));

            var viewComponent =
                new RegistrationsListViewComponent(_facadeServiceMock.Object, _viewComponentHttpContextAccessor.Object);

            var registrationFiltersModel = new RegistrationFiltersModel()
            {
                SearchOrganisationName = SweetsLtdCompanyName,
                IsComplianceSchemeChecked = true,
                IsAcceptedRegistrationChecked = true
            };

            // Act
            var result = await viewComponent.InvokeAsync(new RegistrationListRequest
            {
                SearchOrganisationName = registrationFiltersModel.SearchOrganisationName,
                SearchOrganisationReference = string.Empty,
                IsDirectProducerChecked = true,
                IsComplianceSchemeChecked = false,
                IsPendingRegistrationChecked = false,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = false,
            });

            var model = result.ViewData.Model as RegistrationsListViewModel;

            // Assert
            result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
            model.Should().NotBeNull();
            model.PagedOrganisationRegistrations.Should().BeEquivalentTo(registrations.Items);
            model.PagedOrganisationRegistrations.Count().Should().Be(1);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationName.Should().Be(SweetsLtdCompanyName);
            model.PagedOrganisationRegistrations.FirstOrDefault().OrganisationType.Should()
                .Be(OrganisationType.ComplianceScheme);
            model.PagedOrganisationRegistrations.FirstOrDefault().Decision.Should().Be(ApprovedStatus);
            model.PaginationNavigationModel.CurrentPage.Should().Be(registrations.CurrentPage);
            model.PaginationNavigationModel.PageCount.Should().Be(registrations.TotalPages);
            model.RegulatorRegistrationFiltersModel.SearchOrganisationName.Should().Be(SweetsLtdCompanyName);
            model.RegulatorRegistrationFiltersModel.IsDirectProducerChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsComplianceSchemeChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsPendingRegistrationChecked.Should().BeFalse();
            model.RegulatorRegistrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeTrue();
            model.RegulatorRegistrationFiltersModel.IsRejectedRegistrationChecked.Should().BeFalse();
        }
    }
}