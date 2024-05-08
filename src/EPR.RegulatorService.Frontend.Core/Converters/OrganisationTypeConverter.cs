namespace EPR.RegulatorService.Frontend.Core.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enums;
using Extensions;

public class OrganisationTypeConverter : JsonConverter<OrganisationType>
{
    public override OrganisationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        foreach (OrganisationType type in Enum.GetValues(typeof(OrganisationType)))
        {
            if (type.GetDescription() == value)
            {
                return type;
            }
        }

        throw new JsonException($"Value '{value}' is not valid for {nameof(OrganisationType)}.");
    }

    public override void Write(Utf8JsonWriter writer, OrganisationType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.GetDescription());
}