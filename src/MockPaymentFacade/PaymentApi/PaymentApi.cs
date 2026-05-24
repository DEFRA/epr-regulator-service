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

        // Registration fee resubmission - matched by FileId in request body
        foreach (var filePath in Directory.GetFiles("Responses/PaymentApi/ComplianceSchemeRegistrationFeeResubmission", "*.json"))
        {
            server.Given(Request.Create()
                    .UsingPost()
                    .WithPath("/api/v1/compliance-scheme/registration-fee")
                    .WithBody(new JsonPartialMatcher(JsonSerializer.Serialize(new
                    {
                        fileId = Path.GetFileNameWithoutExtension(filePath),
                    }), ignoreCase: true)))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile(filePath));
        }

        foreach (var filePath in Directory.GetFiles("Responses/PaymentApi/ProducerRegistrationFeeResubmission", "*.json"))
        {
            server.Given(Request.Create()
                    .UsingPost()
                    .WithPath("/api/v1/producer/registration-fee")
                    .WithBody(new JsonPartialMatcher(JsonSerializer.Serialize(new
                    {
                        fileId = Path.GetFileNameWithoutExtension(filePath),
                    }), ignoreCase: true)))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile(filePath));
        }

        // Compliance scheme registration fee endpoints - matched by applicationReferenceNumber in request body
        foreach (var filePath in Directory.GetFiles("Responses/PaymentApi/ComplianceSchemeRegistrationFee", "*.json"))
        {
            server.Given(Request.Create()
                    .UsingPost()
                    .WithPath("/api/v1/compliance-scheme/registration-fee")
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
                .WithPath("/api/v1/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-registration-fee.json"));

        // Offline payments endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/v1/offline-payments"))
            .RespondWith(Response.Create()
                .WithStatusCode(200));

        // Producer resubmission fee endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/v1/producer/resubmission-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-resubmission-fee.json"));

        // Compliance scheme resubmission fees endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/v1/compliance-scheme/resubmission-fees"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/compliance-scheme-resubmission-fees.json"));

        return server;
    }
}
