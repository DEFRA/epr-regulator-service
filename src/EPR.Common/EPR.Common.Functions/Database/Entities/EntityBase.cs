namespace EPR.Common.Functions.Database.Entities;

using System.Text.Json.Serialization;

public abstract class EntityBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; } = Guid.NewGuid();
}