# EPR Regulator Service

## Introduction 
The Regulator Service contains the front end for the regulator portal. This is the main regulator service for EPR with functionality to be added here pertaining to any regulatory activities. 

## Getting Started

### Prerequisites

#### Required software

- Dotnet SDK 6.0

```sh
$ dotnet --version
6.0.413
```

#### epr-packaging-common

In order to restore and build the source code for this project, access to the `epr-packaging-common` package store will need to have been setup.

- Login to Azure DevOps
- Navigate to [Personal Access Tokens](https://dev.azure.com/defragovuk/_usersSettings/tokens)
- Create a new token
    - Enable the `Packaging (Read)` scope

Add the following to your `src/Nuget.Config`

```xml
<packageSourceCredentials>
  <epr-packaging-common>
    <add key="Username" value="<email address>" />
    <add key="ClearTextPassword" value="<personal access token>" />
  </epr-packaging-common>
</packageSourceCredentials>
```

#### Redis

The regulator service requires Redis, the recommended way of runnig Redis is to run it via Docker.

```sh
$ docker run -d --name epr-producers- -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

[Additional information about running services locally is available on confluence](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4326916153/Running+frontend+apps+locally+with+Redis+Azure+b2c+integration)

> When using the regulator facade and backend microservice, Redis will already be running, and a PAT will already have been issued for the `epr-packaging-common` package store so some steps can be skipped.

#### Recommended software

It is recommended that the following repositories have been setup and are running
- [backend-account-microservice](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/backend-account-microservice)
- [epr_regulator_service_facade](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr_regulator_service_facade)

> It is possible to run this project with a mocked facade, so the backend and facade project may not be required.

### Configuration

In order to run the Epr Regulator Service, some configuration will need to be set in `src/EPR.RegulatorService.Frontend.Web/appsettings.development.json`. Configuration properties will be separated by `.` in this documentation.

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

- Open the `src` directory `cd src/`
- Ensure `src/NuGet.config` has been setup correctly for `epr-packaging-common`
- Restore dependencies with `dotnet restore`
- Build the project with `dotnet build`
- Run the Regulator frontend with `dotnet run`

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

## TODO

TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Latest releases
2.	API references

## Contributing to this project
Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence
[Licence information](LICENCE.md).