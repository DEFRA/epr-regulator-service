namespace EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public enum RegulatorTaskType
{
    SiteAddressAndContactDetails,
    WasteLicensesPermitsAndExemptions,
    ReprocessingInputsAndOutputs,
    SamplingAndInspectionPlan,
    RegistrationDulyMade,
    CheckRegistrationStatus,
    AssignOfficer,
    MaterialsAuthorisedOnSite,
    MaterialDetailsAndContact,
    OverseasReprocessorAndInterimSiteDetails,
    BusinessAddress,
    WasteCarrierBrokerDealerNumber,

    // New for accreditation
    PRNTonnage,
    BusinessPlan,
    DulyMade,
    OverseasReprocessingSitesAndEvidenceOfBroadlyEquivalentStandards,
    PERNsTonnageAndAuthorityToIssuePERNs
}