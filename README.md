# EPR Regulator Service

## Introduction

This Regulator Service contains the front end for the regulator portal. This is the main regulator service for [Extended producer responsibility for packaging](https://www.gov.uk/government/collections/extended-producer-responsibility-for-packaging) (EPR).

The application serves as the primary regulatory portal for government regulators across England, Wales, Scotland, and Northern Ireland to oversee organisation registrations, packaging data submissions, and user account management within the EPR ecosystem.

This application supports regulatory officials with different access levels (admin and basic) who need to:

- Review and approve/reject organization applications and enrolments
- Manage packaging data submissions (POM submissions)
- Oversee registration submissions and compliance
- Administer approved persons and delegated users
- Handle reprocessor/exporter registrations and accreditations
- Generate reports and download submission data

## Development

### Prerequisites

- Dotnet SDK v8.0
- NodeJs v24

### NuGet dependencies

This project depends on private nuget feed with a packaged version of [epr-common](https://github.com/DEFRA/epr-common). If you have access to the private feed then you can use [The Azure Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider). If you don't have access to the private feed you can clone epr-common and update the references to point to that.

### Redis

The regulator service requires Redis, the recommended way of running Redis is to run it via Docker.

```sh
$ docker run -d --name epr-producers- -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

[Additional information about running services locally is available on confluence](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4326916153/Running+frontend+apps+locally+with+Redis+Azure+b2c+integration)

> When using the regulator facade and backend microservice, Redis will already be running, and a PAT will already have been issued for the `epr-packaging-common` package store so some steps can be skipped.

### Backing services

This service depends on running copies of the following services, with URLs configured in user-secrets:

- [epr-regulator-service-facade](https://github.com/DEFRA/epr-regulator-service-facade)
- [epr-payment-facade](https://github.com/DEFRA/epr-payment-facade)

### Configuration

In order to run the EPR Regulator Service, some configuration will need to be set in user-secrets. Configuration properties will be separated by `.` in this documentation.

Internal developers can set appropriate user-secrets by using the private [epr-tools-environment-variables](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr-tools-environment-variables?path=%2F&version=GBbigtool-vibe&_a=contents) project.

#### B2C Client 
`AzureAdB2C.ClientSecret` will need to be set for the `ClientId` being used.

#### Facade APIs
If the `epr_regulator_service_facade` has had its port changed, `FacadeApi.BaseUrl` and `EprAuthorizationConfig.FacadeBaseUrl` will need to be updated to reflect this.

#### Landing page urls

If the `frontend-accountmanagement-microservice` is also running locally, `LandingPageUrls.ManageAccountUrl` will need updated to point to whichever port the account management frontend is listening on.

### Installation process

#### Jetbrains Rider

- Open `src/Epr.RegulatorService.sln` in Rider
- Configure `epr-packaging-common` nuget source in Rider
- Build the solution `Build -> Build Solution`
- Run/Debug the `EPR.RegulatorService.Frontend.Web` project `Run -> Run 'EPR.RegulatorService.Frontend.Web: '`

#### .NET CLI

- Run the Regulator frontend with `dotnet run --project src/EPR.RegulatorService.Frontend.Web/EPR.RegulatorService.Frontend.Web.csproj`
- Navigate to https://localhost:7154/regulators/ (note the root url `/` does not work)
- You will need a valid login that is available in both Azure B2C and the configured backend account service.

### Running unit tests

#### Jetbrains Rider

Either open the `Unit Tests` tab and run all unit tests with the `Run All Tests` button, or navigate to `Tests -> Run All Tests For Solution`

#### .NET CLI

- From `src/` run `dotnet test`

```
$ dotnet test
...
Test run for epr_regulator_service\src\EPR.RegulatorService.Frontend.UnitTests\bin\Debug\net6.0\EPR.RegulatorService.Frontend.UnitTests.dll (.NETCoreApp,Version=v6.0)
Microsoft (R) Test Execution Command Line Tool Version 17.3.3 (x64)
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    51, Skipped:     0, Total:    51, Duration: 800 ms - EPR.RegulatorService.Frontend.UnitTests.dll (net6.0)
```

## Contributing to this project
Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence
[Licence information](LICENCE.md).
