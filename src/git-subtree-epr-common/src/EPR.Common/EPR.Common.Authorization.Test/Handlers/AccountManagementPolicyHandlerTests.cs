namespace EPR.Common.Authorization.Test.Handlers;

using Authorization.Handlers;
using Constants;
using Requirements;
using TestClasses;

[TestClass]
public class AccountManagementPolicyHandlerTests
    : PolicyHandlerTestsBase<AccountManagementPolicyHandler<MySession>, AccountManagementPolicyRequirement, MySession>
{
    [TestInitialize]
    public void Initialise() => SetUp();

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    public async Task AccountManagement_Succeeds_WhenUserHasRolesInClaim_AndIsEnrolledAdmin(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenUserHasRolesInClaim(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    public async Task AccountManagement_Succeeds_WhenUserHasRolesInClaim_AndIsEnrolledBasic(
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
    public async Task AccountManagement_Fails_WhenUserIsNotAuthenticated(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserIsNotAuthenticated(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    public async Task AccountManagement_IsNotAuthorised_WhenUserDataExistsInClaimButUserRoleIsNotValid(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataExistsInClaimButUserRoleIsNotAuthorised(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    public async Task AccountManagement_IsNotAuthorised_WhenAuthorizationHandlerContextResourceDoesNotExist() =>
        await HandleRequirementAsync_Fails_WhenAuthorizationHandlerContextResourceDoesNotExist();

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    public async Task AccountManagement_IsAuthorised_WhenCacheContainRequiredUserData(string serviceRole,
        string roleInOrganisation, string enrolmentStatus)
    {
        await HandleRequirementAsync_Succeeds_WhenCacheContainRequiredUserData(serviceRole, roleInOrganisation, enrolmentStatus);
        HttpResponseMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    public async Task AccountManagement_IsAuthorised_WhenCacheContainRequiredUserData_And_RedirectIsSpecified(
        string serviceRole, string roleInOrganisation, string enrolmentStatus)
    {
        SetupSignInRedirect("/regulators/home");
        await HandleRequirementAsync_Succeeds_WhenCacheContainRequiredUserData(serviceRole, roleInOrganisation, enrolmentStatus);
        HttpResponseMock.Verify(response => response.Redirect("/regulators/home"));
    }

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    public async Task AccountManagement_IsNotAuthorised_WhenUserDataExistsInCacheButUserRoleIsNotValid(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataExistsInCacheButUserRoleIsNotAuthorised(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Pending)]
    public async Task AccountManagement_IsAuthorised_WhenClaimsComeFromFromApi(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenUserDataIsRetrievedFromApi(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Pending)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task AccountManagement_IsNotAuthorised_WhenUserDataIsRetrievedFromApiButUserRoleIsNotValid(string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataIsRetrievedFromApiButUserRoleIsNotAuthorised(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    public async Task AccountManagement_ThrowsException_WhenApiCallFails() =>
        await HandleRequirementAsync_ThrowsException_WhenApiCallFails();

    [TestMethod]
    public async Task AccountManagement_Skips_When_Endpoint_Allows_Anonymous_And_User_Unauthenticated() =>
    await HandleRequirementAsync_Skips_WhenEndpointAllowsAnonymous_AndUserUnauthenticated();

    [TestMethod]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Enrolled)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Enrolled)]
    public async Task AccountManagement_Skips_When_Endpoint_Allows_Anonymous_And_User_Authenticated_WouldOtherwiseSucceed(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Skips_WhenEndpointAllowsAnonymous_AndUserAuthenticated_WouldOtherwiseSucceed(
            serviceRole, roleInOrganisation, enrolmentStatus);
}