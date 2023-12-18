namespace EPR.RegulatorService.Frontend.Core.Models;

using System.Text.Json.Serialization;

using CompanyDetails;

public class RegulatorCompanyDetailsModel
{
    [JsonPropertyName("company")]
    public Company Company { get; set; }

    [JsonPropertyName("companyUserInformation")]
    public List<CompanyUserInformation> CompanyUserInformation {get; set;}
}

