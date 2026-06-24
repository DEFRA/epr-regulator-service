namespace IntegrationTests.Features;

internal static class ClrPaymentScenarioIds
{
    internal static readonly Guid Scenario01 = Guid.Parse("2a070c6a-7f3c-4ab2-8848-0cfe176d6ccf");
    internal static readonly Guid Scenario02 = Guid.Parse("74bfb10b-6690-4ed0-b699-3e94d4136b10");
    internal static readonly Guid Scenario03 = Guid.Parse("dd126010-46c8-4519-848b-fdd86142191a");
    internal static readonly Guid Scenario04 = Guid.Parse("87061d73-bb95-4083-8d79-b0cdd05aa4be");
    internal static readonly Guid Scenario05 = Guid.Parse("4233c8f6-9db6-4377-9ef2-b1e0dddbe2a8");
    internal static readonly Guid Scenario06 = Guid.Parse("0c84b7eb-8bf6-4834-bd1f-91e98290cc2b");
    internal static readonly Guid Scenario07 = Guid.Parse("b7e4a1c2-3d5f-6789-abcd-ef0123456789");
    internal static readonly Guid Scenario08 = Guid.Parse("961983b6-dff7-4061-b621-df953a34f9c1");
    internal static readonly Guid Scenario09 = Guid.Parse("c1a2b3d4-e5f6-7890-abcd-ef1234567890");
    internal static readonly Guid Scenario10 = Guid.Parse("d2b3c4e5-f6a7-8901-bcde-f12345678901");
    internal static readonly Guid Scenario11 = Guid.Parse("e3c4d5f6-a7b8-9012-cdef-123456789012");
    internal static readonly Guid Scenario12 = Guid.Parse("f4d5e6a7-b8c9-0123-defa-234567890123");
    internal static readonly Guid Scenario13 = Guid.Parse("a5e6f7b8-c9d0-1234-efab-345678901234");
    internal static readonly Guid Scenario14 = Guid.Parse("b6f7a8c9-d0e1-2345-fabc-456789012345");
    internal static readonly Guid Scenario15 = Guid.Parse("c7a8b9d0-e1f2-3456-abcd-567890123456");

    internal static string AppRef(int scenario) => $"REG-SCENARIO-{scenario:D2}";
}
