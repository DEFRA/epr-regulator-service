namespace IntegrationTests.Features;

/// <summary>Fixed submission IDs for by-submission fee scenarios - same convention as
/// <see cref="ClrPaymentScenarioIds"/>, a distinct range to avoid any collision with it.</summary>
internal static class FeeSnapshotScenarioIds
{
    internal static readonly Guid ProducerHappyPath = Guid.Parse("11a10000-0000-4000-8000-000000000001");
    internal static readonly Guid ComplianceSchemeHappyPath = Guid.Parse("11b10000-0000-4000-8000-000000000002");
    internal static readonly Guid FeatureFlagOff = Guid.Parse("12010000-0000-4000-8000-000000000003");
    internal static readonly Guid ProducerNotFoundFallback = Guid.Parse("13a10000-0000-4000-8000-000000000004");
    internal static readonly Guid ComplianceSchemeNotFoundFallback = Guid.Parse("13b10000-0000-4000-8000-000000000005");
    internal static readonly Guid ProducerServerErrorFallback = Guid.Parse("14010000-0000-4000-8000-000000000006");
    internal static readonly Guid EmptyCsoMembershipDetails = Guid.Parse("15010000-0000-4000-8000-000000000007");
    internal static readonly Guid ProducerSizeFallback = Guid.Parse("16a10000-0000-4000-8000-000000000008");
    internal static readonly Guid MemberTypeFallback = Guid.Parse("16b10000-0000-4000-8000-000000000009");

    internal static string AppRef(string scenario) => $"REG-FEESNAP-{scenario}";
}
