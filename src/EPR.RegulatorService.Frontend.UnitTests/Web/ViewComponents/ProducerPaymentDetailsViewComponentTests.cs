using System;
using System.Threading.Tasks;

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
public class ProducerPaymentDetailsViewComponentTests : ViewComponentsTestBase
{
    private ProducerPaymentDetailsViewComponent _sut;
    private RegistrationSubmissionDetailsViewModel _registrationSumissionDetailsViewModel;
    private readonly Mock<IOptions<PaymentDetailsOptions>> _paymentDetailsOptionsMock = new();
    private readonly Mock<IPaymentFacadeService> _paymentFacadeServiceMock = new();
    private readonly Mock<ILogger<ProducerPaymentDetailsViewComponent>> _loggerMock = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _registrationSumissionDetailsViewModel = new RegistrationSubmissionDetailsViewModel
        {
            ReferenceNumber = "SomeGuid",
            RegistrationDateTime = DateTime.Now.AddDays(-1),
            ProducerDetails = new ProducerDetailsDto()
        };
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _paymentDetailsOptionsMock.Setup(r => r.Value).Returns(new PaymentDetailsOptions());
        _sut = new ProducerPaymentDetailsViewComponent(_paymentDetailsOptionsMock.Object, _paymentFacadeServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_DefaultModel_When_ServiceReturns_Null()
    {
        // Arrange
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as ProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    [DataRow("large", "Large", false)]
    [DataRow("small", "Small", false)]
    [DataRow("large", "Large", true)]
    [DataRow("small", "Small", true)]
    public async Task InvokeAsync_Returns_CorrectView_With_Model(string organisationSize, string expectedProducerSize, bool isResubmission)
    {
        // Arrange
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsAsync(
            It.IsAny<ProducerPaymentRequest>()))
        .ReturnsAsync(new ProducerPaymentResponse // all values in pence
        {
            ApplicationProcessingFee = 100.00M,
            LateRegistrationFee = 200.00M,
            OnlineMarketplaceFee = 300.00M,
            SubsidiaryFee = 400.00M,
            TotalChargeableItems = 1000.00M,
            PreviousPaymentsReceived = 500.00M,
            TotalOutstanding = 500.00M,
            SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                { OnlineMarketPlaceSubsidiariesCount = 1, SubsidiaryOnlineMarketPlaceFee = 200.00M }
        });
        _registrationSumissionDetailsViewModel.ProducerDetails.ProducerType = organisationSize;
        _registrationSumissionDetailsViewModel.IsResubmission = isResubmission;
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1),
            TimeAndDateOfResubmission = DateTime.UtcNow
        };

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as ProducerPaymentDetailsViewModel;
        model.Should().NotBeNull();
        // all values converted to pounds
        model.ApplicationProcessingFee.Should().Be(1.00M);
        model.LateRegistrationFee.Should().Be(2.00M);
        model.OnlineMarketplaceFee.Should().Be(3.00M);
        model.ProducerSize.Should().Be(expectedProducerSize);
        model.SubsidiaryFee.Should().Be(2.00M);
        model.SubsidiaryOnlineMarketPlaceFee.Should().Be(2.00M);
        model.SubTotal.Should().Be(10.00M);
        model.PreviousPaymentsReceived.Should().Be(5.00M);
        model.TotalOutstanding.Should().Be(5.00M);
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Logs_Error_And_Returns_CorrectView_With_DefaultModel_When_Service_Throws()
    {
        // Arrange
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };
        var exception = new Exception("error");
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsAsync(
            It.IsAny<ProducerPaymentRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as ProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to retrieve the producer payment details for")),
                    exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
    }
}