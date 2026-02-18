using Microsoft.AspNetCore.Authorization;

namespace EPR.Common.Authorization.Requirements;

public class AccountPermissionManagementPolicyRequirement : IAuthorizationRequirement
{
    public AccountPermissionManagementPolicyRequirement()
    {
    }
}