namespace EPR.Common.Authorization.Test.Handlers;

using Authorization.Handlers;
using Constants;
using Requirements;
using TestClasses;

[TestClass]
public class AccountPermissionManagementPolicyHandlerTests
    : PolicyHandlerTestsBase<AccountPermissionManagementPolicyHandler<MySession>, AccountPermissionManagementPolicyRequirement, MySession>
{
    [TestInitialize]
    public void Initialise() => SetUp();

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    public async Task AccountPermissionManagement_Succeeds_WhenUserHasRolesInClaim_AndIsEnrolledAdmin(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenUserHasRolesInClaim(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task AccountPermissionManagement_Fails_WhenUserIsNotAuthenticated(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserIsNotAuthenticated(serviceRole, roleInOrganisation, enrolmentStatus);


    [TestMethod]
    public async Task AccountPermissionManagement_ThrowsException_WhenApiCallFails() =>
        await HandleRequirementAsync_ThrowsException_WhenApiCallFails();
}