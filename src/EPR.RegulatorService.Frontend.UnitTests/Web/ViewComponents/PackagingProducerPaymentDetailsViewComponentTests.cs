using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.FeatureManagement;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

[TestClass]
public class PackagingProducerPaymentDetailsViewComponentTests : ViewComponentsTestBase
{
    private PackagingProducerPaymentDetailsViewComponent _sut;
    private SubmissionDetailsViewModel _submissionDetailsViewModel;
    private readonly Mock<IPaymentFacadeService> _paymentFacadeServiceMock = new();
    private readonly Mock<ILogger<PackagingProducerPaymentDetailsViewComponent>> _loggerMock = new();
    private readonly Mock<IOptions<PaymentDetailsOptions>> _paymentDetailsOptionsMock = new();
    private readonly Mock<IFeatureManager> _mockFeatureManager = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionDetailsViewModel = new SubmissionDetailsViewModel
        {
            ReferenceNumber = "SomeGuid",
            NationCode = "gb-eng"
        };
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _paymentDetailsOptionsMock.Setup(r => r.Value).Returns(new PaymentDetailsOptions());
        _mockFeatureManager.Setup(fm =>
                fm.IsEnabledAsync(FeatureFlags.IncludeSubsidiariesInFeeCalculationsForRegulators))
            .ReturnsAsync(false);

        _sut = new PackagingProducerPaymentDetailsViewComponent(
            _paymentDetailsOptionsMock.Object,
            _paymentFacadeServiceMock.Object,
            _mockFeatureManager.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_NoReferenceField_InViewBag()
    {
        // Arrange
        _submissionDetailsViewModel.ReferenceFieldNotAvailable = true;

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentResponse;
        model.Should().BeNull();
        var noReferenceField = _sut.ViewBag.NoReferenceField;
        Assert.IsTrue(noReferenceField);
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(It.IsAny<PackagingProducerPaymentRequest>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_NoReferenceNumber_InViewBag()
    {
        // Arrange
        _submissionDetailsViewModel.ReferenceNotAvailable = true;

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentResponse;
        model.Should().BeNull();
        var noReferenceNumber = _sut.ViewBag.NoReferenceNumber;
        Assert.IsTrue(noReferenceNumber);
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(It.IsAny<PackagingProducerPaymentRequest>()), Times.Never);
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
        var model = result.ViewData.Model as PackagingProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Returns_CorrectView_With_Model()
    {
        // Arrange
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()))
        .ReturnsAsync(new PackagingProducerPaymentResponse // all values in pence
        {
            ResubmissionFee = 10000,
            PreviousPaymentsReceived = 500,
            TotalOutstanding = 9500,
        });

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentDetailsViewModel;
        model.Should().NotBeNull();
        // all values converted to pounds
        model.ResubmissionFee.Should().Be(100.00M);
        model.PreviousPaymentsReceived.Should().Be(5.00M);
        model.TotalOutstanding.Should().Be(95.00M);
        model.ReferenceNumber.Should().Be(_submissionDetailsViewModel.ReferenceNumber);

        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task WhenProducerPaidInExcessOfTheAmountRequiredThenOutstandingPaymentShouldShowZero()
    {
        // Arrange
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()))
        .ReturnsAsync(new PackagingProducerPaymentResponse // all values in pence
        {
            ResubmissionFee = 10000,
            PreviousPaymentsReceived = 500,
            TotalOutstanding = -9500,
        });
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _paymentDetailsOptionsMock.Setup(r => r.Value).Returns(new PaymentDetailsOptions { ShowZeroFeeForTotalOutstanding = true });
        _sut = new PackagingProducerPaymentDetailsViewComponent(
            _paymentDetailsOptionsMock.Object,
            _paymentFacadeServiceMock.Object,
            _mockFeatureManager.Object,
            _loggerMock.Object);

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentDetailsViewModel;
        model.Should().NotBeNull();
        // all values converted to pounds
        model.ResubmissionFee.Should().Be(100.00M);
        model.PreviousPaymentsReceived.Should().Be(5.00M);
        model.TotalOutstanding.Should().Be(0.00M);
        model.ReferenceNumber.Should().Be(_submissionDetailsViewModel.ReferenceNumber);
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Logs_Error_And_Returns_CorrectView_With_DefaultModel_When_Service_Throws()
    {
        // Arrange
        var exception = new Exception("error");
        _paymentFacadeServiceMock.Setup(x => x.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as PackagingProducerPaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetProducerPaymentDetailsForResubmissionAsync(
            It.IsAny<PackagingProducerPaymentRequest>()), Times.AtMostOnce);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to retrieve the packaging producer payment details for")),
                    exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WhenFeatureFlagDisabled_PassesMemberCountAsOne()
    {
        // Arrange
        _submissionDetailsViewModel.MemberCount = 25; // should be ignored when flag disabled
        _paymentFacadeServiceMock
            .Setup(s => s.GetProducerPaymentDetailsForResubmissionAsync(It.IsAny<PackagingProducerPaymentRequest>()))
            .ReturnsAsync(new PackagingProducerPaymentResponse
            {
                ResubmissionFee = 0,
                PreviousPaymentsReceived = 0,
                TotalOutstanding = 0
            });

        // Act
        await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        _paymentFacadeServiceMock.Verify(s =>
            s.GetProducerPaymentDetailsForResubmissionAsync(
                It.Is<PackagingProducerPaymentRequest>(r => r.MemberCount == 1)), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_WhenFeatureFlagEnabled_Passes_ViewModel_MemberCount()
    {
        // Arrange
        _submissionDetailsViewModel.MemberCount = 37;
        _mockFeatureManager.Setup(fm =>
                fm.IsEnabledAsync(FeatureFlags.IncludeSubsidiariesInFeeCalculationsForRegulators))
            .ReturnsAsync(true);

        _paymentFacadeServiceMock
            .Setup(s => s.GetProducerPaymentDetailsForResubmissionAsync(It.IsAny<PackagingProducerPaymentRequest>()))
            .ReturnsAsync(new PackagingProducerPaymentResponse
            {
                ResubmissionFee = 0,
                PreviousPaymentsReceived = 0,
                TotalOutstanding = 0
            });

        // Act
        await _sut.InvokeAsync(_submissionDetailsViewModel);

        // Assert
        _paymentFacadeServiceMock.Verify(s =>
            s.GetProducerPaymentDetailsForResubmissionAsync(
                It.Is<PackagingProducerPaymentRequest>(r => r.MemberCount == 37)), Times.Once);
    }
}