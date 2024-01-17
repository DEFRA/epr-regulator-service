using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.MockedData
{
    public static class MockedEnrolmentRequestDetails
    {
        public static OrganisationEnrolments GetMockedEnrolmentRequestDetails() => new()
        {
            OrganisationId = Guid.NewGuid(),
            OrganisationName = "Test Org Worldwide.",
            OrganisationReferenceNumber = "123245",
            OrganisationType = "CompaniesHouseCompany",
            CompaniesHouseNumber = "123456",
            IsComplianceScheme = false,
            NationId = 1,
            NationName = "England",
            BusinessAddress = new BusinessAddress()
            {
                BuildingName = "test house",
                BuildingNumber = "1",
                Street = "test street",
                County = "test county",
                PostCode = "ABC 123",
            },
            Users = new List<User>()
            {
                new()
                {
                    FirstName = "James",
                    LastName = "Tester",
                    Email = "jtester@test.com",
                    JobTitle = "Director",
                    TelephoneNumber = "020 111 111",
                    IsEmployeeOfOrganisation = true,
                    Enrolment = new Enrolment
                    {
                        ServiceRole = "Packaging.ApprovedPerson",
                        EnrolmentStatus = "Approved",
                        ExternalId = Guid.NewGuid()
                    }
                },
                new()
                {
                    FirstName = "Peter",
                    LastName = "Tester",
                    Email = "ptester@test.com",
                    JobTitle = "Tester",
                    TelephoneNumber = "020 222 333",
                    IsEmployeeOfOrganisation = true,
                    Enrolment = new Enrolment
                    {
                        ServiceRole = "Packaging.DelegatedPerson",
                        EnrolmentStatus = "Pending",
                        ExternalId = Guid.NewGuid()
                    }
                },
                new()
                {
                    FirstName = "Angela",
                    LastName = "Tester",
                    Email = "atester@test.com",
                    JobTitle = "Developer",
                    TelephoneNumber = "020 333 444",
                    IsEmployeeOfOrganisation = true,
                    Enrolment = new Enrolment
                    {
                        ServiceRole = "Packaging.DelegatedPerson",
                        EnrolmentStatus = "Pending",
                        ExternalId = Guid.NewGuid()
                    }
                }
            }
        };
    }   
}