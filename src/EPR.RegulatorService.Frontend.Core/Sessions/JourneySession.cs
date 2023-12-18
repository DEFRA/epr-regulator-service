using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

[ExcludeFromCodeCoverage]
public class JourneySession : IHasUserData
{
    public UserData UserData { get; set; } = new();
    public RegulatorSession RegulatorSession { get; set; } = new();

    public RegulatorSubmissionSession RegulatorSubmissionSession { get; set; }= new();
    public PermissionManagementSession PermissionManagementSession { get; set; } = new();

    public bool IsComplianceScheme { get; set; }
    public SearchManageApproversSession SearchManageApproversSession { get; set; } = new();

    public RemoveApprovedUserSession RemoveApprovedUserSession { get; set; } = new();

}