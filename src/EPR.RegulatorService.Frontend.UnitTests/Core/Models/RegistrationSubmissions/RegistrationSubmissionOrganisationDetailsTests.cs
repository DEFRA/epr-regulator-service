namespace EPR.RegulatorService.Frontend.UnitTests.Core.Models.RegistrationSubmissions;

[TestClass]
public sealed class RegistrationSubmissionOrganisationDetailsTests
{
    [TestMethod]
    public void ImplicitOperator_FromResponse_Maps_PaycalParameters_ToProducerDetails()
    {
        var response = new RegistrationSubmissionOrganisationDetailsResponse
        {
            SubmissionDetails = new RegistrationSubmissionOrganisationSubmissionSummaryDetails(),
            OrganisationSize = "large",
            NumberOfSubsidiariesClosedLoopRecycling = 11,
            NumberOfHoldingCompaniesClosedLoopRecycling = 2
        };

        RegistrationSubmissionOrganisationDetails details = response;

        Assert.AreEqual("large", details.ProducerDetails.ProducerType);
        Assert.AreEqual(11, details.ProducerDetails.NumberOfSubsidiariesClosedLoopRecycling);
        Assert.AreEqual(2, details.ProducerDetails.NumberOfHoldingCompaniesClosedLoopRecycling);
    }
}
