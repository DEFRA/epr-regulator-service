# EPR Regulator Service - Outside-In Integration Tests

This test project demonstrates an outside-in testing approach for the EPR Regulator Service frontend microservice.

By testing only the inputs and outputs of the microservice we can isolate the tests from internal implementation changes and prove behaviour of the service as a whole while controlling behaviour of all external dependencies.

## Structure

- WebApplicationFactory builds and runs real application for testing
- WireMocks provide fake backing microservices
- Azure B2C auth faked by replacing the auth services with a `TestAuthHandler` under control of the tests, allowing us to control auth success/failure and returned claims during testing. (See [AuthMock.md](AuthMock.md))
- Return HTML asserted on via "page model" classes allowing us to decouple tests from changes in HTML structures

## Debugging

To see the logs from the captive web host enable verbose logging as follows:

```sh
dotnet test --filter ManageRegistrationSubmissionsTests --logger "console;verbosity=detailed"
```

## References

- [Integration tests in ASP.NET Core | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0&pivots=xunit)
- [SMAL-286 Add outside-in tests of regulator-frontend microservice](https://eaflood.atlassian.net/browse/SMAL-286)
