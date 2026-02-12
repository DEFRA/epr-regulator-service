using MockPaymentFacade;

Console.WriteLine("Starting Mock Payment Facade Server...");
var server = MockPaymentFacadeServer.Start(port: 7107, useSsl: true);
Console.WriteLine($"Mock Payment Facade running at: {server.Url}");
Console.WriteLine("Press any key to stop...");
Console.ReadKey();
server.Stop();
