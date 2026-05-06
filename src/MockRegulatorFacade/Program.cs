namespace MockRegulatorFacade;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class Program
{
    private static void Main()
    {
        const int port = 7253; // same as regulator-facade when run locally, to make it easy to flip between them
        Console.WriteLine($"MockRegulatorFacade starting on https://localhost:{port}");
        MockRegulatorFacadeServer.Start(port: port, useSsl: true);

        Console.WriteLine("Press any key to stop.");
        Console.ReadKey();
    }
}
