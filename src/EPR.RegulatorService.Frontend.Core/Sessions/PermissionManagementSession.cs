using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

[ExcludeFromCodeCoverage]
public class  PermissionManagementSession
{
    public List<PermissionManagementSessionItem> Items { get; set; } = new();
}