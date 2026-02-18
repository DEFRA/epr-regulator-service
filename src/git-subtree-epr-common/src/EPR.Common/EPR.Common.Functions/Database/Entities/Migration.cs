namespace EPR.Common.Functions.Database.Entities;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Migration
{
    public string MigrationId { get; set; }

    public string ProductVersion { get; set; }
}