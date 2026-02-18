namespace EPR.Common.Authorization.Test.Helpers;

using System.Security.Claims;
using System.Text.Json;
using Authorization.Extensions;
using Authorization.Helpers;
using Constants;
using Microsoft.Identity.Web;
using Models;

[TestClass]
public class ClaimsPrincipleHelperTests
{
    private ClaimsPrincipal _claimsPrinciple;
    private ClaimsIdentity _claimsIdentity;

    [TestInitialize]
    public void Setup()
    {
        _claimsIdentity = new ClaimsIdentity();
        _claimsPrinciple = new ClaimsPrincipal(_claimsIdentity);
    }

    [TestMethod]
    public void CanSubmitForOrganisation_ReturnsFalse_WhenNoUserDataClaim()
    {
        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void CanSubmitForOrganisation_ReturnsTrue_WhenApprovedPersonRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.ApprovedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void CanSubmitForOrganisation_ReturnsTrue_WhenDelegatedUserRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.DelegatedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void CanSubmitForOrganisation_ReturnsFalse_WhenInvalidRole()
    {
        var userData = new UserData { ServiceRole = "InvalidRole" };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedDelegatedOrBasicPerson_ReturnsFalse_WhenNoUserDataClaim()
    {
        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedDelegatedOrBasicPerson(_claimsPrinciple));
    }

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson)]
    [DataRow(ServiceRoles.BasicUser)]
    [DataRow(ServiceRoles.DelegatedPerson)]
    public void IsApprovedDelegatedOrBasicPerson_ReturnsTrue_WhenValidPersonRole(string serviceRole)
    {
        var userData = new UserData { ServiceRole = serviceRole };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsApprovedDelegatedOrBasicPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedDelegatedOrBasicPerson_ReturnsFalse_WhenInvalidRole()
    {
        var userData = new UserData { ServiceRole = "InvalidRole" };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedDelegatedOrBasicPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void CanUploadFilesForOrganisation_ReturnsFalse_WhenNoUserDataClaim()
    {
        Assert.IsFalse(ClaimsPrincipleHelper.CanUploadFilesForOrganisation(_claimsPrinciple));
    }

    [TestMethod]
    public void CanUploadFilesForOrganisation_ReturnsTrue_WhenApprovedPersonRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.ApprovedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.CanUploadFilesForOrganisation(_claimsPrinciple));
    }

    [TestMethod]
    public void CanUploadFilesForOrganisation_ReturnsTrue_WhenDelegatedUserRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.DelegatedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.CanUploadFilesForOrganisation(_claimsPrinciple));
    }

    [TestMethod]
    public void CanUploadFilesForOrganisation_ReturnsFalse_WhenInvalidRole()
    {
        var userData = new UserData { ServiceRole = "InvalidRole" };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.CanUploadFilesForOrganisation(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedOrDelegatedPerson_ReturnsFalse_WhenNoUserDataClaim()
    {
        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void IsEnrolledAdmin_ReturnsFalse_WhenNoUserDataClaim()
    {
        Assert.IsFalse(ClaimsPrincipleHelper.IsEnrolledAdmin(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedOrDelegatedPerson_ReturnsTrue_WhenApprovedPersonRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.ApprovedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    [DataRow(EnrolmentStatuses.Approved)]
    [DataRow(EnrolmentStatuses.Enrolled)]
    [DataRow(EnrolmentStatuses.Pending)]
    public void IsEnrolledAdmin_ReturnsTrue_WhenAdminRoleAndApproved_Enrolled_or_Pending(string enrolmentStatus)
    {
        var userData = new UserData { RoleInOrganisation = RoleInOrganisation.Admin, EnrolmentStatus = enrolmentStatus };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsEnrolledAdmin(_claimsPrinciple));
    }

    [TestMethod]
    [DataRow(EnrolmentStatuses.Invited)]
    [DataRow(EnrolmentStatuses.NotSet)]
    [DataRow(EnrolmentStatuses.Rejected)]
    public void IsEnrolledAdmin_ReturnsFalse_WhenAdminRoleAndInvited_NotSet_or_Rejected(string enrolmentStatus)
    {
        var userData = new UserData { RoleInOrganisation = RoleInOrganisation.Admin, EnrolmentStatus = enrolmentStatus };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.IsEnrolledAdmin(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedOrDelegatedPerson_ReturnsTrue_WhenDelegatedUserRole()
    {
        var userData = new UserData { ServiceRole = ServiceRoles.DelegatedPerson };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsTrue(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void IsApprovedOrDelegatedPerson_ReturnsFalse_WhenInvalidRole()
    {
        var userData = new UserData { ServiceRole = "InvalidRole" };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson(_claimsPrinciple));
    }

    [TestMethod]
    public void IsEnrolledAdmin_ReturnsFalse_WhenInvalidRole()
    {
        var userData = new UserData { ServiceRole = "InvalidRole" };
        _claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        Assert.IsFalse(ClaimsPrincipleHelper.IsEnrolledAdmin(_claimsPrinciple));
    }
}