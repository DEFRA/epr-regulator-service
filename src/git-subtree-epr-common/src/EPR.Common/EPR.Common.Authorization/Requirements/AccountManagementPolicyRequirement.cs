using Microsoft.AspNetCore.Authorization;

namespace EPR.Common.Authorization.Requirements;

public class AccountManagementPolicyRequirement : IAuthorizationRequirement
{
    public AccountManagementPolicyRequirement()
    {
    }
}