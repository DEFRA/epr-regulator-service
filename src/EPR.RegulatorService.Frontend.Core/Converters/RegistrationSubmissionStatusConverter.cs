namespace EPR.RegulatorService.Frontend.Core.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionStatusConverter : JsonConverter<RegistrationSubmissionStatus>
{
    public override RegistrationSubmissionStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        foreach (RegistrationSubmissionStatus type in Enum.GetValues(typeof(RegistrationSubmissionStatus)))
        {
            if (type.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
            {
                return type;
            }
        }

        throw new JsonException($"Value '{value}' is not valid for {nameof(OrganisationType)}.");
    }

    public override void Write(Utf8JsonWriter writer, RegistrationSubmissionStatus value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
