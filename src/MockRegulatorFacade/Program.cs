namespace MockRegulatorFacade;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class Program
{
    private static void Main()
    {
        const int port = 7253;
        Console.WriteLine($"MockRegulatorFacade starting on http://localhost:{port}");
        MockRegulatorFacadeServer.Start(port: port);

        Console.WriteLine("Press any key to stop.");
        Console.ReadKey();
    }
}
