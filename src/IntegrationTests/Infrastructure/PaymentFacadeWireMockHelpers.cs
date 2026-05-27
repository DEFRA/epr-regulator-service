namespace IntegrationTests.Infrastructure;

using System.Text.Json;

internal static class PaymentFacadeWireMockHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
