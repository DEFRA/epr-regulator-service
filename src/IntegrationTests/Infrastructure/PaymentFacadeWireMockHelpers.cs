namespace IntegrationTests.Infrastructure;

using System.Text.Json;
using WireMock;
using WireMock.Server;

/// <summary>
/// Helpers for asserting outbound payment-facade HTTP calls recorded by WireMock.
/// </summary>
internal static class PaymentFacadeWireMockHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns the JSON body from the most recent POST whose path contains <paramref name="pathFragment"/>
    /// and whose body includes <paramref name="bodyMustContain"/>.
    /// </summary>
    public static string GetLastPostBody(IWireMockServer server, string pathFragment, string bodyMustContain)
    {
        var body = server.LogEntries
            .Reverse()
            .Select(e => e.RequestMessage)
            .FirstOrDefault(r =>
                string.Equals(r.Method, "POST", StringComparison.OrdinalIgnoreCase)
                && r.Path?.Contains(pathFragment, StringComparison.OrdinalIgnoreCase) == true
                && GetBody(r)?.Contains(bodyMustContain, StringComparison.Ordinal) == true)
            is { } request
            ? GetBody(request)
            : null;

        return body ?? throw new InvalidOperationException(
            $"No POST to a path containing '{pathFragment}' with '{bodyMustContain}' in the body.");
    }

    private static string? GetBody(IRequestMessage request) =>
        !string.IsNullOrEmpty(request.Body) ? request.Body : request.BodyData?.BodyAsString;
}
