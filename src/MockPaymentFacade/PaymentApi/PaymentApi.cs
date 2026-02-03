namespace MockPaymentFacade.PaymentApi;

using System.Text.Json;

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

        // Compliance scheme registration fee endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    complianceSchemeRegistrationFee = 262000,
                    totalFee = 165800,
                    previousPayment = 0,
                    outstandingPayment = 165800,
                    complianceSchemeMembersWithFees = new[]
                    {
                        new
                        {
                            memberId = "100001",
                            memberType = "large",
                            memberRegistrationFee = 165800,
                            memberOnlineMarketPlaceFee = 0,
                            memberLateRegistrationFee = 0,
                            subsidiariesFee = 0,
                            totalMemberFee = 165800
                        }
                    }
                })));

        // Producer registration fee endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    producerRegistrationFee = 165800,
                    producerOnlineMarketPlaceFee = 0,
                    producerLateRegistrationFee = 0,
                    totalFee = 165800,
                    previousPayment = 0,
                    outstandingPayment = 165800,
                    subsidiariesFee = 0,
                    subsidiariesFeeBreakdown = new
                    {
                        totalSubsidiariesOMPFees = 0,
                        countOfOMPSubsidiaries = 0
                    }
                })));

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
                .WithBody(JsonSerializer.Serialize(new
                {
                    producerResubmissionFee = 0,
                    previousPayment = 0,
                    outstandingPayment = 0
                })));

        // Compliance scheme resubmission fees endpoint
        server.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/resubmission-fees"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    totalResubmissionFee = 0,
                    previousPayment = 0,
                    outstandingPayment = 0,
                    memberResubmissionFees = Array.Empty<object>()
                })));

        return server;
    }
}
