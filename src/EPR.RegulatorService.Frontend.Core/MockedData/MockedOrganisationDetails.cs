namespace EPR.RegulatorService.Frontend.Core.MockedData;

using System.Diagnostics.CodeAnalysis;

using Models;
using Models.CompanyDetails;

[ExcludeFromCodeCoverage]
public static class MockedOrganisationDetails
{
    public static RegulatorCompanyDetailsModel GetMockedOrganisationDetails() => new()
    {
        Company = new Company
        {
            OrganisationId = Guid.NewGuid().ToString(),
            OrganisationTypeId = 2,
            CompaniesHouseNumber = "NI622515",
            IsComplianceScheme = false,
            OrganisationName = "Test Company",
            RegisteredAddress = new RegisteredAddress
            {
                BuildingName = "test house",
                BuildingNumber = "1",
                Street = "test street",
                County = null,
                Postcode = "ABC 123",
                SubBuildingName = null,
                Locality = null,
                DependentLocality = null,
                Town = "Test Town",
                Country = null
            }
        },
       CompanyUserInformation = new List<CompanyUserInformation>
       {
           new()
           {
               FirstName = "",
               LastName = "",
               Email = "test.account+L1003@kainos.com",
               PersonRoleId = 2,
               UserEnrolments = new List<CompanyEnrolments>
               {
                   new()
                   {
                       ServiceRoleId = 3,
                       EnrolmentStatusId = 1
                   }
               },
               IsEmployee = false,
               JobTitle = "",
               PhoneNumber = "",
               ExternalId = Guid.NewGuid()
           }
       }
    };
}