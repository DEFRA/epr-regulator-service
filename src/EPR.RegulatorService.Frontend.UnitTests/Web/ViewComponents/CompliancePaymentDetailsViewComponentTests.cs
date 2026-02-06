using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

[TestClass]
public class CompliancePaymentDetailsViewComponentTests : ViewComponentsTestBase
{
    private CompliancePaymentDetailsViewComponent _sut;
    private RegistrationSubmissionDetailsViewModel _registrationSumissionDetailsViewModel;
    private readonly Mock<IOptions<PaymentDetailsOptions>> _paymentDetailsOptionsMock = new();
    private readonly Mock<IPaymentFacadeService> _paymentFacadeServiceMock = new();
    private readonly Mock<ILogger<CompliancePaymentDetailsViewComponent>> _loggerMock = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _registrationSumissionDetailsViewModel = new RegistrationSubmissionDetailsViewModel
        {
            ReferenceNumber = "SomeGuid",
            RegistrationDateTime = DateTime.Now.AddDays(-1),
            NationId = 1
        };
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _paymentDetailsOptionsMock.Setup(r => r.Value).Returns(new PaymentDetailsOptions());
        _sut = new CompliancePaymentDetailsViewComponent(_paymentDetailsOptionsMock.Object, _paymentFacadeServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_DefaultModel_When_ServiceReturns_Null()
    {
        // Arrange
        _registrationSumissionDetailsViewModel.CSOMembershipDetails = [];
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as CompliancePaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InvokeAsync_Returns_CorrectView_With_Model(bool isResubmission)
    {
        // Arrange
        var complianceSchemeMembers = new List<CsoMembershipDetailsDto> {
            new() { MemberId = "memberid1", MemberType = "large" },
            new() { MemberId = "memberid2", MemberType = "small" }
        };
        _registrationSumissionDetailsViewModel.CSOMembershipDetails = complianceSchemeMembers;
        _registrationSumissionDetailsViewModel.IsResubmission = isResubmission;
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1),
            TimeAndDateOfResubmission = DateTime.UtcNow
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
        .ReturnsAsync(new CompliancePaymentResponse // all values in pence
        {
            ApplicationProcessingFee = 100.00M, 
            TotalChargeableItems = 1000.00M,
            PreviousPaymentsReceived = 500.00M,
            TotalOutstanding = 500.00M,
            ComplianceSchemeMembers =
            [
                new() { MemberId = "memberid1", MemberType = "large", MemberFee = 2.00M },
                new() { MemberId = "memberid2", MemberType = "small", MemberFee = 3.00M }
            ]
        });

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as CompliancePaymentDetailsViewModel;
        model.Should().NotBeNull();
        model.ApplicationFee.Should().NotBe(null);
        model.SubTotal.Should().NotBe(null);
        model.TotalOutstanding.Should().NotBe(null);
        model.LargeProducerCount.Should().Be(1);
        model.SmallProducerCount.Should().Be(1);
        model.LateProducerCount.Should().Be(0);
        model.OnlineMarketPlaceCount.Should().Be(0);
        model.SubsidiariesCompanyCount.Should().Be(0);
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_Exception_When_Service_Throws()
    {
        // Arrange
        var complianceSchemeMembers = new List<CsoMembershipDetailsDto> {
            new() { MemberId = "memberid1", MemberType = "large" },
            new() { MemberId = "memberid2", MemberType = "small" }
        };
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1),
            TimeAndDateOfResubmission = DateTime.UtcNow
        };
        _registrationSumissionDetailsViewModel.CSOMembershipDetails = complianceSchemeMembers;
        var exception = new Exception("error");
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
            await _sut.InvokeAsync(_registrationSumissionDetailsViewModel));

        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()), Times.Once);
    }
}