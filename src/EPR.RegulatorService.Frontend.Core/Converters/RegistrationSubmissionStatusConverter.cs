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
        var values = Enum.GetValues(typeof(RegistrationSubmissionStatus)).OfType<RegistrationSubmissionStatus>().ToList();
        if (string.IsNullOrEmpty(value) || !Enum.TryParse(typeof(RegistrationSubmissionStatus), value!, true, out _))
        {
            throw new JsonException($"Value '{value}' is not valid for {nameof(OrganisationType)}.");
        }

        return values.Single(o => o.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase));
    }

    public override void Write(Utf8JsonWriter writer, RegistrationSubmissionStatus value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
