using System.Diagnostics.CodeAnalysis;
using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

[ExcludeFromCodeCoverage]
public class JourneySession : IHasUserData
{
    public UserData UserData { get; set; } = new();
    public RegulatorSession RegulatorSession { get; set; } = new();
    public RegulatorSubmissionSession RegulatorSubmissionSession { get; set; } = new();
    public RegulatorRegistrationSubmissionSession RegulatorRegistrationSubmissionSession { get; set; } = new();
    public PermissionManagementSession PermissionManagementSession { get; set; } = new();
    public bool IsComplianceScheme { get; set; }
    public SearchManageApproversSession SearchManageApproversSession { get; set; } = new();
    public AddRemoveApprovedUserSession AddRemoveApprovedUserSession { get; set; } = new();
    public RegulatorRegistrationSession RegulatorRegistrationSession { get; set; } = new();
    public InviteNewApprovedPersonSession InviteNewApprovedPersonSession { get; set; } = new();
}