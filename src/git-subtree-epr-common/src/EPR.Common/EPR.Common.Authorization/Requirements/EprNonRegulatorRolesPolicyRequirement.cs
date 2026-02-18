namespace EPR.Common.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

public class EprNonRegulatorRolesPolicyRequirement : IAuthorizationRequirement
{
    public EprNonRegulatorRolesPolicyRequirement()
    {
    }
}