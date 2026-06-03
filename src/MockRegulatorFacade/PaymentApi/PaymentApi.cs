namespace MockRegulatorFacade.PaymentApi;

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

        var producerFeeByReferenceDir = Path.Combine("Responses/PaymentApi/ProducerRegistrationFee");
        if (Directory.Exists(producerFeeByReferenceDir))
        {
            foreach (var filePath in Directory.GetFiles(producerFeeByReferenceDir, "*.json"))
            {
                server.Given(Request.Create()
                        .UsingPost()
                        .WithPath("/producer/registration-fee")
                        .WithBody(new JsonPartialMatcher(JsonSerializer.Serialize(new
                        {
                            applicationReferenceNumber = Path.GetFileNameWithoutExtension(filePath),
                        }))))
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyFromFile(filePath));
            }
        }

        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-registration-fee.json"));

        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/offline-payments"))
            .RespondWith(Response.Create()
                .WithStatusCode(200));

        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/resubmission-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/PaymentApi/producer-resubmission-fee.json"));

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
