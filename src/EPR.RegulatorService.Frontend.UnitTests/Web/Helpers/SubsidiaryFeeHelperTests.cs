using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.Helpers;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers;

[TestClass]
public class SubsidiaryFeeHelperTests
{
    [TestMethod]
    public void GetNetSubsidiaryCompaniesFee_WhenBreakdownIsNull_ReturnsSubsidiaryFee()
    {
        SubsidiaryFeeHelper.GetNetSubsidiaryCompaniesFee(50_000m, null)
            .Should().Be(50_000m);
    }

    [TestMethod]
    public void GetNetSubsidiaryCompaniesFee_WhenBreakdownPresent_ExcludesOmpAndClr()
    {
        var breakdown = new SubsidiariesFeeBreakdownResponse
        {
            SubsidiaryOnlineMarketPlaceFee = 10_000m,
            TotalSubsidiariesClosedLoopRecyclingFees = 15_000m
        };

        SubsidiaryFeeHelper.GetNetSubsidiaryCompaniesFee(100_000m, breakdown)
            .Should().Be(75_000m);
    }

    [TestMethod]
    public void GetNetSubsidiaryCompaniesFee_OnProducerPaymentResponse_DelegatesToSharedHelper()
    {
        var response = new ProducerPaymentResponse
        {
            SubsidiaryFee = 100_000m,
            SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
            {
                SubsidiaryOnlineMarketPlaceFee = 20_000m,
                TotalSubsidiariesClosedLoopRecyclingFees = 30_000m
            }
        };

        response.GetNetSubsidiaryCompaniesFee().Should().Be(50_000m);
    }
}
