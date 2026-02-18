namespace EPR.Common.Functions.AccessControl.MockAuthentication.MockData;

using Models;

public static class MockUserGenerator
{
    public static UserInformation[] Generate => new[]
    {
        new UserInformation
        {
            Email = "john.doe@here.com",
            Password = "password1",
            FirstName = "John",
            LastName = "Doe",
            UniqueReference = Guid.NewGuid(),
            CustomerOrganisationId = Guid.Parse("9a994eed-7a62-4e10-84cb-cf7280cfdf98"),
            CustomerId = Guid.Parse("f8f246d0-2d44-454b-8af8-4e93085649d9"),
        },
        new UserInformation
        {
            Email = "jane.doe@here.com",
            Password = "password1",
            FirstName = "Jane",
            LastName = "Doe",
            UniqueReference = Guid.NewGuid(),
            CustomerOrganisationId = Guid.Parse("b5de1f34-aaf2-46b4-9fee-cbb24a2a6899"),
            CustomerId = Guid.Parse("3763dcbc-5e30-4ad3-ae7d-d477681a57bc"),
        },
        new UserInformation
        {
            Email = "sam.doe@here.com",
            Password = "password1",
            FirstName = "Sam",
            LastName = "Doe",
            UniqueReference = Guid.NewGuid(),
            CustomerOrganisationId = Guid.Parse("b5de1f34-aaf2-46b4-9fee-cbb24a2a6899"),
            CustomerId = Guid.Parse("661b067f-7da1-4b37-a0e4-3a1f8387f973"),
        },
    };
}