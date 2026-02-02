namespace MockRegulatorFacade.FacadeApi;

using System;
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

    public static WireMockServer WithFacadeApi(this WireMockServer server, Guid submissionId)
    {
        // Ensure the common stubs are present too
        server.WithFacadeApi();

        server.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{submissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/FacadeApi/organisation-registration-submission-details.json"));

        return server;
    }
}
