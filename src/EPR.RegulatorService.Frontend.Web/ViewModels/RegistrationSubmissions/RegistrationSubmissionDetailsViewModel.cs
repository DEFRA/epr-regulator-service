namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Collections.Generic;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

public class RegistrationSubmissionDetailsViewModel
{
    public Guid SubmissionId { get; set; }

    public Guid OrganisationId { get; set; }

    public string OrganisationReference { get; set; }

    public string OrganisationName { get; set; }

    public string? ApplicationReferenceNumber { get; set; }

    public string? RegistrationReferenceNumber { get; set; }

    public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

    public BusinessAddress BusinessAddress { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public string RegisteredNation { get; set; }

    public int NationId { get; set; }

    public string NationCode { get; set; }

    public string PowerBiLogin { get; set; }

    public RegistrationSubmissionStatus Status { get; set; }

    public SubmissionDetailsViewModel SubmissionDetails { get; set; }

    public DateTime RegistrationDateTime { get; set; }

    public int RegistrationYear { get; set; }

    public string? ProducerComments { get; set; }

    public string? RegulatorComments { get; set; }

    public string? RejectReason { get; set; } = string.Empty;

    public string? CancellationReason { get; set; } = string.Empty;

    public int NoOfSubsidiariesOnlineMarketPlace { get; set; }

    public int NoOfSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }

    public string OrganisationSize { get; set; }

    public List<CsoMembershipDetailsDto> CSOMembershipDetails { get; set; }

    // Implicit operator from RegistrationSubmissionOrganisationDetails to RegistrationSubmissionDetailsViewModel
    public static implicit operator RegistrationSubmissionDetailsViewModel(RegistrationSubmissionOrganisationDetails details) => details is null ? null : new RegistrationSubmissionDetailsViewModel
    {
        SubmissionId = details.SubmissionId,
        OrganisationId = details.OrganisationId,
        OrganisationReference = details.OrganisationReference[..Math.Min(details.OrganisationReference.Length, 10)],
        OrganisationName = details.OrganisationName,
        ApplicationReferenceNumber = details.ApplicationReferenceNumber,
        RegistrationReferenceNumber = details.RegistrationReferenceNumber,
        OrganisationSize = details.OrganisationSize,
        OrganisationType = details.OrganisationType,
        CompaniesHouseNumber = details.CompaniesHouseNumber,
        RegisteredNation = details.Country, // Assuming RegisteredNation corresponds to the Country
        NationId = details.NationId,
        NationCode = details.NationCode,
        Status = details.SubmissionStatus,
        RegistrationDateTime = details.SubmissionDate,
        RegistrationYear = details.RelevantYear,
        RegulatorComments = details.RegulatorComments,
        ProducerComments = details.ProducerComments,
        BusinessAddress = new BusinessAddress
        {
            BuildingName = details.BuildingName,
            SubBuildingName = details.SubBuildingName,
            BuildingNumber = details.BuildingNumber,
            Street = details.Street,
            Town = details.Town,
            County = details.County,
            Country = details.Country,
            PostCode = details.Postcode
        },
        SubmissionDetails = details.SubmissionDetails,
        RejectReason = details.RejectReason,
        CancellationReason = details.CancellationReason,
        IsLateFeeApplicable = details.IsLateSubmission,
        NoOfSubsidiaries = details.NumberOfSubsidiaries,
        NoOfSubsidiariesOnlineMarketPlace = details.NumberOfOnlineSubsidiaries,
        IsProducerOnlineMarketplace = details.IsOnlineMarketPlace,
        CSOMembershipDetails = details.CsoMembershipDetails
    };

    // Implicit operator from RegistrationSubmissionDetailsViewModel to RegistrationSubmissionOrganisationDetails  
    public static implicit operator RegistrationSubmissionOrganisationDetails(RegistrationSubmissionDetailsViewModel details) => details is null ? null : new RegistrationSubmissionOrganisationDetails
    {
        SubmissionId = details.SubmissionId,
        OrganisationId = details.OrganisationId,
        OrganisationReference = details.OrganisationReference[..Math.Min(details.OrganisationReference.Length, 10)],
        OrganisationName = details.OrganisationName,
        ApplicationReferenceNumber = details.ApplicationReferenceNumber,
        RegistrationReferenceNumber = details.RegistrationReferenceNumber,
        OrganisationSize = details.OrganisationSize,
        OrganisationType = details.OrganisationType,
        CompaniesHouseNumber = details.CompaniesHouseNumber,
        Country = details.RegisteredNation,
        SubmissionStatus = details.Status,
        SubmissionDate = details.RegistrationDateTime,
        RelevantYear = details.RegistrationYear,
        RegulatorComments = details.RegulatorComments,
        ProducerComments = details.ProducerComments,
        BuildingName = details.BusinessAddress?.BuildingName,
        SubBuildingName = details.BusinessAddress?.SubBuildingName,
        BuildingNumber = details.BusinessAddress?.BuildingNumber,
        Street = details.BusinessAddress?.Street,
        Town = details.BusinessAddress?.Town,
        County = details.BusinessAddress?.County,
        Postcode = details.BusinessAddress?.PostCode,
        SubmissionDetails = details.SubmissionDetails,
        NationCode = details.NationCode,
        NationId = details.NationId,
        RejectReason = details.RejectReason,
        CancellationReason = details.CancellationReason,
        NumberOfOnlineSubsidiaries = details.NoOfSubsidiariesOnlineMarketPlace,
        NumberOfSubsidiaries = details.NoOfSubsidiaries,
        IsLateSubmission = details.IsLateFeeApplicable,
        IsOnlineMarketPlace = details.IsProducerOnlineMarketplace,
        CsoMembershipDetails = details.CSOMembershipDetails
    };
}
