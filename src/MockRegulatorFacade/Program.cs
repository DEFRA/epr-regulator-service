namespace MockRegulatorFacade;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class Program
{
    private static void Main()
    {
        Console.WriteLine("MockRegulatorFacade starting on http://localhost:9092");

        MockRegulatorFacadeServer.Start(port: 9092);

        Console.WriteLine("Press any key to stop.");
        Console.ReadKey();
    }
}
