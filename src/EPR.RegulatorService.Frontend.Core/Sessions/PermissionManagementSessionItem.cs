using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

[ExcludeFromCodeCoverage]
public class PermissionManagementSessionItem
{
    public List<string> Journey { get; set; } = new();

    public Guid Id { get; set; }

    public PermissionType? PermissionType { get; set; }
    
    public RelationshipWithOrganisation RelationshipWithOrganisation { get; set; }
    
    public string? AdditionalRelationshipInformation { get; set; }
    
    public string? NameOfConsultancy { get; set; }

    public string? NameOfOrganisation { get; set; }

    public string? NameOfComplianceScheme { get; set; }

    public string? JobTitle { get; set; }
    
    public string? Fullname { get; set; }
}
