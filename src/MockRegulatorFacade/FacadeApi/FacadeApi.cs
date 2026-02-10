namespace MockRegulatorFacade.FacadeApi;

using System;
using System.IO;
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

        server.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/organisations/pending-applications"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/FacadeApi/pending-applications.json"));

        server.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/pom/get-submissions"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile("Responses/FacadeApi/pom-submissions.json"));

        var registrationSubmissionDetailsPath = "Responses/FacadeApi/RegistrationSubmissionDetails";
        if (!Directory.Exists(registrationSubmissionDetailsPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {registrationSubmissionDetailsPath}");
        }

        foreach (var filePath in Directory.GetFiles(registrationSubmissionDetailsPath, "*.json"))
        {
            var submissionGuid = Path.GetFileNameWithoutExtension(filePath);
            if (!Guid.TryParse(submissionGuid, out _))
            {
                throw new InvalidOperationException($"File name is not a valid GUID: {filePath}");
            }

            server.Given(Request.Create()
                    .UsingGet()
                    .WithPath($"/api/organisation-registration-submission-details/{submissionGuid}"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile(filePath));
        }

        var organisationEnrolmentsPath = "Responses/FacadeApi/OrganisationEnrolments";
        if (!Directory.Exists(organisationEnrolmentsPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {organisationEnrolmentsPath}");
        }

        foreach (var filePath in Directory.GetFiles(organisationEnrolmentsPath, "*.json"))
        {
            var organisationGuid = Path.GetFileNameWithoutExtension(filePath);
            if (!Guid.TryParse(organisationGuid, out _))
            {
                throw new InvalidOperationException($"File name is not a valid GUID: {filePath}");
            }

            server.Given(Request.Create()
                    .UsingGet()
                    .WithPath($"/api/organisations/{organisationGuid}/pending-applications"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile(filePath));
        }

        return server;
    }
}
