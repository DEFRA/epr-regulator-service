namespace EPR.Common.Functions.Database.Entities;

using System.Diagnostics.CodeAnalysis;
using Interfaces;

[ExcludeFromCodeCoverage]
public abstract class AuditEntityBase : EntityBase, IAudit
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    public Guid _Id { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

    public int EntityState { get; set; }

    public Guid? UserId { get; set; }

    public string EmailAddress { get; set; }

    public DateTime AuditCreated { get; set; }
}