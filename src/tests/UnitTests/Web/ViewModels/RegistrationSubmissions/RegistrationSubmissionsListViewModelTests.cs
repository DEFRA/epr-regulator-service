namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class RegistrationSubmissionsListViewModelTests
    {
        private readonly RegistrationSubmissionsListViewModel _sut = new RegistrationSubmissionsListViewModel();

        [TestMethod]
        public void WhenTheFilterView_IsntPopulated_TheFilterIsCreatedBlank()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = false,
                Is2026Checked = false,
                IsOrganisationComplianceChecked = false,
                IsOrganisationLargeChecked = false,
                IsOrganisationSmallChecked = false,
                IsStatusCancelledChecked = false,
                IsStatusGrantedChecked = false,
                IsStatusPendingChecked = false,
                IsStatusQueriedChecked = false,
                IsStatusRefusedChecked = false,
                IsStatusUpdatedChecked = false,
                OrganisationName = null,
                OrganisationRef = null
            };

            _sut.RegistrationsFilterModel = emptyFilterViewModel;

            var result = (RegistrationSubmissionsFilterModel)_sut.RegistrationsFilterModel;
            result.OrganisationName.Should().BeNull();
            result.OrganisationReference.Should().BeNull();
            result.OrganisationType.Should().BeEmpty();
            result.Statuses.Should().BeEmpty();
            result.RelevantYears.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenTheFilterView_IsntPopulated_AndPageNumberIsntSupplied_TheFilterIsCreatedBlank_AtPageNumber1()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = false,
                Is2026Checked = false,
                IsOrganisationComplianceChecked = false,
                IsOrganisationLargeChecked = false,
                IsOrganisationSmallChecked = false,
                IsStatusCancelledChecked = false,
                IsStatusGrantedChecked = false,
                IsStatusPendingChecked = false,
                IsStatusQueriedChecked = false,
                IsStatusRefusedChecked = false,
                IsStatusUpdatedChecked = false,
                OrganisationName = null,
                OrganisationRef = null
            };

            _sut.RegistrationsFilterModel = emptyFilterViewModel;
            var result = (RegistrationSubmissionsFilterModel)_sut.RegistrationsFilterModel;
            result.PageNumber.Should().Be(1);
        }

        [TestMethod]
        public void WhenTheFilterView_IsntPopulated_AndPageNumberIsSupplied_TheFilterIsCreatedBlank_AtThatPageNumber()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = false,
                Is2026Checked = false,
                IsOrganisationComplianceChecked = false,
                IsOrganisationLargeChecked = false,
                IsOrganisationSmallChecked = false,
                IsStatusCancelledChecked = false,
                IsStatusGrantedChecked = false,
                IsStatusPendingChecked = false,
                IsStatusQueriedChecked = false,
                IsStatusRefusedChecked = false,
                IsStatusUpdatedChecked = false,
                OrganisationName = null,
                OrganisationRef = null,
                PageNumber = 2
            };

            _sut.RegistrationsFilterModel = emptyFilterViewModel;
            var result = (RegistrationSubmissionsFilterModel)_sut.RegistrationsFilterModel;
            result.PageNumber.Should().Be(2);
        }

        [TestMethod]
        public void WhenTheFilterView_IsPopulated_AndPageNumberIsSupplied_TheFilterIsCreatedWith_Corresponding__Values()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = true,
                Is2026Checked = false,
                IsOrganisationComplianceChecked = true,
                IsOrganisationLargeChecked = true,
                IsOrganisationSmallChecked = false,
                IsStatusCancelledChecked = true,
                IsStatusGrantedChecked = false,
                IsStatusPendingChecked = true,
                IsStatusQueriedChecked = false,
                IsStatusRefusedChecked = false,
                IsStatusUpdatedChecked = true,
                OrganisationName = "Org name",
                OrganisationRef = "Org ref",
                PageNumber = 2
            };

            _sut.RegistrationsFilterModel = emptyFilterViewModel;

            var result = (RegistrationSubmissionsFilterModel)_sut.RegistrationsFilterModel;

            result.OrganisationName.Should().Be("Org name");
            result.OrganisationReference.Should().Be("Org ref");
            result.OrganisationType.Should().Contain("compliance");
            result.OrganisationType.Should().Contain("large");
            result.Statuses.Should().Contain("Pending");
            result.Statuses.Should().Contain("Cancelled");
            result.Statuses.Should().Contain("Updated");
            result.RelevantYears.Should().Contain("2025");
            result.RelevantYears.Should().NotContain("2026");

            result.PageNumber.Should().Be(2);
        }
    }
}
