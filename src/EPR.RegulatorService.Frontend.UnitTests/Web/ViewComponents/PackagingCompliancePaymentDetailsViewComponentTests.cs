using System;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

[TestClass]
public class PackagingCompliancePaymentDetailsViewComponentTests : ViewComponentsTestBase
{
    private PackagingCompliancePaymentDetailsViewComponent _sut;
    private SubmissionDetailsViewModel _submissionDetailsViewModel;
    private readonly Mock<IPaymentFacadeService> _paymentFacadeServiceMock = new();
    private readonly Mock<ILogger<PackagingCompliancePaymentDetailsViewComponent>> _loggerMock = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionDetailsViewModel = new SubmissionDetailsViewModel
        {
            ReferenceNumber = "SomeGuid",
            RegistrationDateTime = DateTime.Now.AddDays(-1),
            NationCode = "GB-ENG"
        };
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new PackagingCompliancePaymentDetailsViewComponent(_paymentFacadeServiceMock.Object, _loggerMock.Object);
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
        var model = result.ViewData.Model as PackagingCompliancePaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsForResubmissionAsync(It.IsAny<PackagingCompliancePaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_Model()
    {
        // Arrange
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingCompliancePaymentRequest>()))
        .ReturnsAsync(new PackagingCompliancePaymentResponse // all values in pence
        {
            ResubmissionFee = 10000,
            PreviousPaymentsReceived = 500,
            TotalOutstanding = 9500
        });

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingCompliancePaymentDetailsViewModel;
        model.Should().NotBeNull();
        model.ResubmissionFee.Should().Be(100.00M);
        model.PreviousPaymentReceived.Should().Be(5.00M);
        model.TotalOutstanding.Should().Be(95.00M);
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingCompliancePaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Logs_Error_And_Returns_CorrectView_With_DefaultModel_When_Service_Throws()
    {
        // Arrange
        var exception = new Exception("error");
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingCompliancePaymentRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingCompliancePaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingCompliancePaymentRequest>()), Times.AtMostOnce);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Unable to retrieve the packaging compliance scheme payment details for {_submissionDetailsViewModel.SubmissionId}")),
                    exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
    }
}