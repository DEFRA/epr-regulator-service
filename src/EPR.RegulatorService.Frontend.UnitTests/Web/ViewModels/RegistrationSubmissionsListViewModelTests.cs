namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var result = _sut.CreateFilterModel(1);
            result.OrganisationName.Should().BeNull();
            result.OrganisationRef.Should().BeNull();
            result.OrganisationType.Should().BeEmpty();
            result.SubmissionStatus.Should().BeEmpty();
            result.RelevantYear.Should().BeNull();
        }

        [TestMethod]
        public void WhenTheFilterView_IsntPopulated_AndPageNumberIsntSupplied_TheFilterIsCreatedBlank_AtPageNumber1()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = false,
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
            var result = _sut.CreateFilterModel(null);
            result.Page.Should().Be(1);
        }

        [TestMethod]
        public void WhenTheFilterView_IsntPopulated_AndPageNumberIsSupplied_TheFilterIsCreatedBlank_AtThatPageNumber()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = false,
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
            var result = _sut.CreateFilterModel(2);
            result.Page.Should().Be(2);
        }

        [TestMethod]
        public void WhenTheFilterView_IsPopulated_AndPageNumberIsSupplied_TheFilterIsCreatedWith_Corresponding__Values()
        {
            var emptyFilterViewModel = new RegistrationSubmissionsFilterViewModel
            {
                Is2025Checked = true,
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
                OrganisationRef = "Org ref"
            };

            _sut.RegistrationsFilterModel = emptyFilterViewModel;

            var result = _sut.CreateFilterModel(2);

            result.OrganisationName.Should().Be("Org name");
            result.OrganisationRef.Should().Be("Org ref");
            result.OrganisationType.Should().Contain("compliance");
            result.OrganisationType.Should().Contain("large");
            result.SubmissionStatus.Should().Contain("pending");
            result.SubmissionStatus.Should().Contain("cancelled");
            result.SubmissionStatus.Should().Contain("updated");
            result.RelevantYear.Should().Contain("2025");

            result.Page.Should().Be(2);
        }
    }
}
