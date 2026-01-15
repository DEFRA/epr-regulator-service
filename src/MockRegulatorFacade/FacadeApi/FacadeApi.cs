namespace MockRegulatorFacade.FacadeApi;

using System.Diagnostics.CodeAnalysis;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public static class FacadeApi
{
    public static WireMockServer WithFacadeApi(this WireMockServer server)
    {
        server.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/user-accounts"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/FacadeApi/user-accounts.json"));

        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/organisation-registration-submissions"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/FacadeApi/organisation-registration-submissions.json"));

        return server;
    }
}
