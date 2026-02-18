using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Handlers;
using EPR.Common.Authorization.Requirements;
using EPR.Common.Authorization.Test.TestClasses;

namespace EPR.Common.Authorization.Test.Handlers;

[TestClass]
public class RegulatorBasicPolicyHandlerTests : PolicyHandlerTestsBase<RegulatorBasicPolicyHandler<MySession>,
    RegulatorBasicPolicyRequirement, MySession>
{
    [TestInitialize]
    public void Initialise() => SetUp();

    [TestMethod]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsAuthorised_WhenUserHasRolesInClaim(string serviceRole, string roleInOrganisation,
        string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenUserHasRolesInClaim(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsNotAuthorised_WhenUserIsNotAuthenticated(string serviceRole,
        string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserIsNotAuthenticated(serviceRole, roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsNotAuthorised_WhenUserDataExistsInClaimButUserServiceRoleIsNotValid(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataExistsInClaimButUserRoleIsNotAuthorised(serviceRole,
            roleInOrganisation, enrolmentStatus);

    [TestMethod]
    public async Task RegulatorBasic_IsNotAuthorised_WhenAuthorizationHandlerContextResourceDoesNotExist() =>
        await HandleRequirementAsync_Fails_WhenAuthorizationHandlerContextResourceDoesNotExist();

    [TestMethod]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsAuthorised_WhenContextDoesNotContainRequiredClaims(string serviceRole,
        string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenCacheContainRequiredUserData(serviceRole, roleInOrganisation,
            enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsNotAuthorised_WhenUserDataExistsInCacheButUserServiceRoleIsNotValid(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataExistsInCacheButUserRoleIsNotAuthorised(serviceRole,
            roleInOrganisation, enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorAdmin, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsAuthorised_WhenClaimsComeFromFromApi(string serviceRole, string roleInOrganisation,
        string enrolmentStatus) =>
        await HandleRequirementAsync_Succeeds_WhenUserDataIsRetrievedFromApi(serviceRole, roleInOrganisation,
            enrolmentStatus);

    [TestMethod]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.NotSet, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.BasicUser, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.ApprovedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.DelegatedPerson, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_IsNotAuthorised_WhenUserDataIsRetrievedFromApiButUserServiceRoleIsNotValid(
        string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
        await HandleRequirementAsync_Fails_WhenUserDataIsRetrievedFromApiButUserRoleIsNotAuthorised(serviceRole,
            roleInOrganisation, enrolmentStatus);

    [TestMethod]
    public async Task RegulatorBasic_ThrowsException_WhenApiCallFails() =>
        await HandleRequirementAsync_ThrowsException_WhenApiCallFails();

    [TestMethod]
    public async Task RegulatorBasic_Skips_When_Endpoint_Allows_Anonymous_And_User_Unauthenticated() =>
        await HandleRequirementAsync_Skips_WhenEndpointAllowsAnonymous_AndUserUnauthenticated();

    [TestMethod]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.NotSet, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Admin, EnrolmentStatuses.Approved)]
    [DataRow(ServiceRoles.RegulatorBasic, RoleInOrganisation.Employee, EnrolmentStatuses.Approved)]
    public async Task RegulatorBasic_Skips_When_Endpoint_Allows_Anonymous_And_User_Authenticated_WouldOtherwiseSucceed(
    string serviceRole, string roleInOrganisation, string enrolmentStatus) =>
    await HandleRequirementAsync_Skips_WhenEndpointAllowsAnonymous_AndUserAuthenticated_WouldOtherwiseSucceed(
        serviceRole, roleInOrganisation, enrolmentStatus);



}
