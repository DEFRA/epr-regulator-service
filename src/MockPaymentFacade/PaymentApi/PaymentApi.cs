namespace MockPaymentFacade.PaymentApi;

using System;
using System.IO;
using System.Text.Json;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public static class PaymentApi
{
    public static WireMockServer WithPaymentApi(this WireMockServer server)
    {
        // Root endpoint to show something helpful in the browser when run with launchSettings
        server.Given(Request.Create()
                .UsingGet()
                .WithPath("/"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("MockPaymentFacade up.\nReady to process API requests."));

        // Compliance scheme registration fee endpoints - matched by applicationReferenceNumber in request body
        foreach (var filePath in Directory.GetFiles("Responses/PaymentApi/ComplianceSchemeRegistrationFee", "*.json"))
        {
            server.Given(Request.Create()
                    .UsingPost()
                    .WithPath("/compliance-scheme/registration-fee")
                    .WithBody(new JsonPartialMatcher(JsonSerializer.Serialize(new
                    {
                        applicationReferenceNumber = Path.GetFileNameWithoutExtension(filePath),
                    }))))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile(filePath));
        }

        // Producer registration fee endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-registration-fee.json"));

        // Offline payments endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/offline-payments"))
            .RespondWith(Response.Create()
                .WithStatusCode(200));

        // Producer resubmission fee endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/resubmission-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-resubmission-fee.json"));

        // Compliance scheme resubmission fees endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/resubmission-fees"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/compliance-scheme-resubmission-fees.json"));

        return server;
    }
}
