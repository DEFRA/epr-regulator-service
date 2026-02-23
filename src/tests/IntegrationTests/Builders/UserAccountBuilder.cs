namespace IntegrationTests.Builders;

public class UserAccountBuilder
{
    private string _organisationName = "Environment Agency";
    private int _nationId = 1;
    private string _firstName = "Test";
    private string _lastName = "User";

    private UserAccountBuilder()
    {
    }

    public static UserAccountBuilder Default() => new();

    public UserAccountBuilder WithOrganisationName(string organisationName)
    {
        _organisationName = organisationName;
        return this;
    }

    public UserAccountBuilder WithNationId(int nationId)
    {
        _nationId = nationId;
        return this;
    }

    public UserAccountBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserAccountBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public object Build() => new
    {
        user = new
        {
            id = "62309b0e-535d-4f96-9a3b-9c759a3944f3",
            firstName = _firstName,
            lastName = _lastName,
            email = "test.user@example.com",
            roleInOrganisation = "Admin",
            enrolmentStatus = "Approved",
            serviceRole = "Regulator Basic",
            service = "RegulatorService",
            serviceRoleId = 5,
            organisations = new[]
            {
                new
                {
                    id = "C7646CAE-EB96-48AC-9427-0120199BE6EE",
                    name = _organisationName,
                    organisationRole = "Regulator",
                    organisationType = "Regulators",
                    nationId = _nationId,
                },
            },
        },
    };
}
