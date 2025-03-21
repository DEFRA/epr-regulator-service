namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class RegistrationSubmissionsFilterTests
{
    [TestMethod]
    public void ViewModel_WillClearValues_When_ClearFilters_Is_Set_To_True()
    {
        // Arrange
        var viewModel = new RegistrationSubmissionsFilterViewModel
        {
            IsOrganisationComplianceChecked = true,
            IsOrganisationSmallChecked = true,
            IsOrganisationLargeChecked = true,
            IsStatusCancelledChecked = true,
            IsStatusGrantedChecked = true,
            IsStatusPendingChecked = true,
            IsStatusQueriedChecked = true,
            IsStatusRefusedChecked = true,
            IsStatusUpdatedChecked = true,
            IsResubmissionAcceptedRegistrationChecked = true,
            IsResubmissionRejectedRegistrationChecked = true,
            IsResubmissionPendingRegistrationChecked = true,
            PageNumber = 2,
            Is2025Checked = true,
            // Act
            ClearFilters = true
        };

        // Assert
        Assert.IsFalse(viewModel.IsOrganisationComplianceChecked, "IsOrganisationComplianceChecked should be false");
        Assert.IsFalse(viewModel.IsOrganisationSmallChecked, "IsOrganisationSmallChecked should be false");
        Assert.IsFalse(viewModel.IsOrganisationLargeChecked, "IsOrganisationLargeChecked should be false");
        Assert.IsFalse(viewModel.IsStatusCancelledChecked, "IsStatusCancelledChecked should be false");
        Assert.IsFalse(viewModel.IsStatusGrantedChecked, "IsStatusGrantedChecked should be false");
        Assert.IsFalse(viewModel.IsStatusPendingChecked, "IsStatusPendingChecked should be false");
        Assert.IsFalse(viewModel.IsStatusQueriedChecked, "IsStatusQueriedChecked should be false");
        Assert.IsFalse(viewModel.IsStatusRefusedChecked, "IsStatusRefusedChecked should be false");
        Assert.IsFalse(viewModel.IsStatusUpdatedChecked, "IsStatusUpdatedChecked should be false");
        Assert.IsFalse(viewModel.IsResubmissionAcceptedRegistrationChecked, "IsResubmissionAcceptedRegistrationChecked should be false");
        Assert.IsFalse(viewModel.IsResubmissionRejectedRegistrationChecked, "IsResubmissionRejectedRegistrationChecked should be false");
        Assert.IsFalse(viewModel.IsResubmissionPendingRegistrationChecked, "IsResubmissionPendingRegistrationChecked should be false");

        Assert.IsFalse(viewModel.Is2025Checked, "Is2025Checked should be false");
    }

    [TestMethod]
    public void ViewModelToModel_MultipleOrganisationTypeFlags_ShouldMapCorrectly()
    {
        // Arrange
        var viewModel = new RegistrationSubmissionsFilterViewModel
        {
            IsOrganisationComplianceChecked = true,
            IsOrganisationSmallChecked = true,
            IsOrganisationLargeChecked = false, // Only two flags set
            PageNumber = 2,
            Is2025Checked = true
        };

        // Act
        RegistrationSubmissionsFilterModel model = viewModel;

        // Assert
        Assert.AreEqual("compliance small", model.OrganisationType);
        Assert.AreEqual(2, model.PageNumber);
        Assert.AreEqual("2025", model.RelevantYears);
    }

    [TestMethod]
    public void ViewModelToModel_MultipleSubmissionStatusFlags_ShouldMapCorrectly()
    {
        // Arrange
        var viewModel = new RegistrationSubmissionsFilterViewModel
        {
            IsStatusGrantedChecked = true,
            IsStatusRefusedChecked = true,
            IsStatusPendingChecked = false,
            IsStatusQueriedChecked = true,
            IsStatusUpdatedChecked = false,
            IsStatusCancelledChecked = true
        };

        // Act
        RegistrationSubmissionsFilterModel model = viewModel;

        // Assert
        Assert.AreEqual("Cancelled Granted Queried Refused", model.Statuses);
    }

    [TestMethod]
    public void ViewModelToModel_EmptyFields_ShouldReturnNullOrEmptyProperties()
    {
        // Arrange
        var viewModel = new RegistrationSubmissionsFilterViewModel
        {
            OrganisationName = string.Empty,
            OrganisationRef = null,
            IsOrganisationComplianceChecked = false,
            IsOrganisationSmallChecked = false,
            IsOrganisationLargeChecked = false,
            PageNumber = 1
        };

        // Act
        RegistrationSubmissionsFilterModel model = viewModel;

        // Assert
        Assert.IsNull(model.OrganisationName);
        Assert.IsNull(model.OrganisationReference);
        Assert.AreEqual(string.Empty, model.OrganisationType);
    }

    [TestMethod]
    public void ModelToViewModel_MultipleOrganisationTypeValues_ShouldSetCorrectFlags()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            OrganisationType = "compliance small",
            RelevantYears = "2025"
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsTrue(viewModel.IsOrganisationComplianceChecked);
        Assert.IsTrue(viewModel.IsOrganisationSmallChecked);
        Assert.IsFalse(viewModel.IsOrganisationLargeChecked);
        Assert.IsTrue(viewModel.Is2025Checked);
    }

    [TestMethod]
    public void ModelToViewModel_MultipleSubmissionStatusValues_ShouldSetCorrectFlags()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            Statuses = "granted refused queried cancelled"
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsTrue(viewModel.IsStatusGrantedChecked);
        Assert.IsTrue(viewModel.IsStatusRefusedChecked);
        Assert.IsTrue(viewModel.IsStatusQueriedChecked);
        Assert.IsTrue(viewModel.IsStatusCancelledChecked);
        Assert.IsFalse(viewModel.IsStatusPendingChecked);
        Assert.IsFalse(viewModel.IsStatusUpdatedChecked);
    }

    [TestMethod]
    public void ModelToViewModel_MultipleResubmissionStatusesValues_ShouldSetCorrectFlags()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            ResubmissionStatuses = "pending accepted rejected"
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsTrue(viewModel.IsResubmissionAcceptedRegistrationChecked);
        Assert.IsTrue(viewModel.IsResubmissionPendingRegistrationChecked);
        Assert.IsTrue(viewModel.IsResubmissionRejectedRegistrationChecked);
    }
    [TestMethod]
    public void ModelToViewModel_PartialResubmissionStatusesValues_ShouldSetCorrectFlags()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            ResubmissionStatuses = "pending accepted"
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsTrue(viewModel.IsResubmissionAcceptedRegistrationChecked);
        Assert.IsTrue(viewModel.IsResubmissionPendingRegistrationChecked);
        Assert.IsFalse(viewModel.IsResubmissionRejectedRegistrationChecked);
    }


    [TestMethod]
    public void ModelToViewModel_EmptyOrNullFields_ShouldMapToDefaults()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            OrganisationName = null,
            OrganisationType = string.Empty,
            Statuses = null,
            ResubmissionStatuses = null,
            PageNumber = null
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsNull(viewModel.OrganisationName);
        Assert.IsFalse(viewModel.IsOrganisationComplianceChecked);
        Assert.IsFalse(viewModel.IsOrganisationSmallChecked);
        Assert.IsFalse(viewModel.IsOrganisationLargeChecked);
        Assert.IsFalse(viewModel.IsResubmissionAcceptedRegistrationChecked);
        Assert.IsFalse(viewModel.IsResubmissionPendingRegistrationChecked);
        Assert.IsFalse(viewModel.IsResubmissionRejectedRegistrationChecked);

        Assert.AreEqual(1, viewModel.PageNumber); // Default page number
    }

    [TestMethod]
    public void ModelToViewModel_PartialSubmissionStatusValues_ShouldSetCorrectFlags()
    {
        // Arrange
        var model = new RegistrationSubmissionsFilterModel
        {
            Statuses = "granted pending"
        };

        // Act
        RegistrationSubmissionsFilterViewModel viewModel = model;

        // Assert
        Assert.IsTrue(viewModel.IsStatusGrantedChecked);
        Assert.IsTrue(viewModel.IsStatusPendingChecked);
        Assert.IsFalse(viewModel.IsStatusRefusedChecked);
        Assert.IsFalse(viewModel.IsStatusCancelledChecked);
    }
}
