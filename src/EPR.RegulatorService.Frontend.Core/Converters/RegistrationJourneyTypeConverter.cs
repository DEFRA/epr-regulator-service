namespace EPR.RegulatorService.Frontend.Core.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Enums;

internal class RegistrationJourneyTypeConverter : JsonConverter<RegistrationJourneyType>
{
    public override RegistrationJourneyType Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        foreach (RegistrationJourneyType type in Enum.GetValues(typeof(RegistrationJourneyType)))
        {
            if (type.ToString() == value)
            {
                return type;
            }
        }

        throw new JsonException($"Value '{value}' is not valid for {nameof(RegistrationJourneyType)}.");
    }

    public override void Write(Utf8JsonWriter writer, RegistrationJourneyType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}