using System;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

[TestClass]
public class PackagingProducerPaymentDetailsViewComponentTests : ViewComponentsTestBase
{
    private PackagingProducerPaymentDetailsViewComponent _sut;
    private SubmissionDetailsViewModel _submissionDetailsViewModel;
    private readonly Mock<IPaymentFacadeService> _paymentFacadeServiceMock = new();
    private readonly Mock<ILogger<PackagingProducerPaymentDetailsViewComponent>> _loggerMock = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionDetailsViewModel = new SubmissionDetailsViewModel
        {
            ApplicationReferenceNumber = "SomeGuid",
            RegistrationDateTime = DateTime.Now.AddDays(-1),
            ProducerDetails = new ProducerDetailsDto()
        };
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new PackagingProducerPaymentDetailsViewComponent(_paymentFacadeServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_DefaultModel_When_ServiceReturns_Null()
    {
        // Arrange

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as ProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    [DataRow("large", "Large")]
    [DataRow("small", "Small")]
    public async Task InvokeAsync_Returns_CorrectView_With_Model(string organisationSize, string expectedProducerSize)
    {
        // Arrange
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(
            It.IsAny<ProducerPaymentRequest>()))
        .ReturnsAsync(new PackagingProducerPaymentResponse // all values in pence
        {
            ResubmissionFee = 10000,
            PreviousPaymentsReceived = 500,
            TotalOutstanding = 9500,
            TotalChargeableItems = 10000
        });
        _submissionDetailsViewModel.ProducerDetails.ProducerType = organisationSize;

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentDetailsViewModel;
        model.Should().NotBeNull();
        // all values converted to pounds
        model.ResubmissionFee.Should().Be(100.00M);
        model.SubTotal.Should().Be(100.00M);
        model.PreviousPaymentsReceived.Should().Be(5.00M);
        model.TotalOutstanding.Should().Be(95.00M);
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Logs_Error_And_Returns_CorrectView_With_DefaultModel_When_Service_Throws()
    {
        // Arrange
        var exception = new Exception("error");
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(
            It.IsAny<ProducerPaymentRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(
            It.IsAny<ProducerPaymentRequest>()), Times.AtMostOnce);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to retrieve the packaging producer payment details for")),
                    exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
    }
}