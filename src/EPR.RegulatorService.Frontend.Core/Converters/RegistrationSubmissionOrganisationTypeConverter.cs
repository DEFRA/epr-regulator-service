namespace EPR.RegulatorService.Frontend.Core.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionOrganisationTypeConverter : JsonConverter<RegistrationSubmissionOrganisationType>
{
    public override RegistrationSubmissionOrganisationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        foreach (RegistrationSubmissionOrganisationType type in Enum.GetValues(typeof(RegistrationSubmissionOrganisationType)))
        {
            if (type.ToString() == value)
            {
                return type;
            }
        }

        throw new JsonException($"Value '{value}' is not valid for {nameof(OrganisationType)}.");
    }

    public override void Write(Utf8JsonWriter writer, RegistrationSubmissionOrganisationType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
