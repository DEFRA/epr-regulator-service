namespace MockRegulatorFacade;

using System.Diagnostics.CodeAnalysis;

using FacadeApi;

using WireMock.Net.StandAlone;
using WireMock.Server;
using WireMock.Settings;

[ExcludeFromCodeCoverage]
public static class MockRegulatorFacadeServer
{
    public static WireMockServer Start(int? port = null)
    {
        var settings = new WireMockServerSettings();
        if (port.HasValue)
        {
            settings.Port = port.Value;
        }

        var server = StandAloneApp.Start(settings)
            .WithFacadeApi();

        return server;
    }
}
