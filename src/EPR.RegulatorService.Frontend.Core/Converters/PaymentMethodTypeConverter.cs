using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Converters;

public class PaymentMethodTypeConverter : JsonConverter<PaymentMethodType>
{
    public override PaymentMethodType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return PaymentMethodType.AllTypes.SingleOrDefault(type => type.ToString() == value)
            ?? throw new JsonException($"Unknown PaymentMethodType value: {value}");
    }

    public override void Write(Utf8JsonWriter writer, PaymentMethodType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}