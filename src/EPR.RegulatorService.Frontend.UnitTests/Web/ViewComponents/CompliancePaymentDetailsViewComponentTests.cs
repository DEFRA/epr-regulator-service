using EPR.RegulatorService.Frontend.Core.Enums;
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
            NationId = 1,
            ProducerDetails = new ProducerDetailsDto()
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
        _paymentFacadeServiceMock.Verify(
            r => r.GetCompliancePaymentDetailsAsync(It.Is<CompliancePaymentRequest>(c =>
                c.FileId == _registrationSumissionDetailsViewModel.ResubmissionFileId)), Times.AtMostOnce);
    }

    [TestMethod]
    public async Task InvokeAsync_Logs_Error_And_Returns_CorrectView_With_DefaultModel_When_Service_Throws()
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

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewViewComponentResult>();
        var model = result.ViewData.Model as CompliancePaymentResponse;
        model.Should().BeNull();
        _paymentFacadeServiceMock.Verify(r => r.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()), Times.AtMostOnce);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Unable to retrieve the compliance scheme payment details for {_registrationSumissionDetailsViewModel.SubmissionId}")),
                    exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InvokeAsync_Passes_NumberOfSubsidiariesClosedLoopRecycling_ToPaymentFacade(bool isResubmission)
    {
        // Arrange
        CompliancePaymentRequest? capturedRequest = null;
        _registrationSumissionDetailsViewModel.ReferenceNumber = "REF-CLOSED-LOOP";
        _registrationSumissionDetailsViewModel.NationCode = "GB-ENG";
        _registrationSumissionDetailsViewModel.RegistrationJourneyType = RegistrationJourneyType.CsoLargeProducer;
        _registrationSumissionDetailsViewModel.IsResubmission = isResubmission;
        _registrationSumissionDetailsViewModel.CSOMembershipDetails =
        [
            new CsoMembershipDetailsDto
            {
                MemberId = "memberid1",
                MemberType = "large",
                NumberOfHoldingCompaniesClosedLoopRecycling = 1,
                NumberOfSubsidiariesClosedLoopRecycling = 7
            }
        ];
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1),
            TimeAndDateOfResubmission = DateTime.UtcNow
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .Callback<CompliancePaymentRequest>(r => capturedRequest = r)
            .ReturnsAsync(new CompliancePaymentResponse
            {
                ApplicationProcessingFee = 100.00M,
                TotalChargeableItems = 1000.00M,
                PreviousPaymentsReceived = 500.00M,
                TotalOutstanding = 500.00M,
                ComplianceSchemeMembers =
                [
                    new() { MemberId = "memberid1", MemberType = "large", MemberFee = 2.00M }
                ]
            });

        // Act
        await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        capturedRequest.Should().NotBeNull();
        var members = capturedRequest!.ComplianceSchemeMembers!.ToList();
        members.Should().ContainSingle();
        members[0].NoOfSubsidiariesClosedLoopRecycling.Should().Be(7);
        members[0].NoOfHoldingCompaniesClosedLoopRecycling.Should().Be(1);
        members[0].IsClosedLoopRecycling.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_Zeros_ClosedLoopCounts_For_CsoSmallProducer()
    {
        // Arrange
        CompliancePaymentRequest? capturedRequest = null;
        _registrationSumissionDetailsViewModel.RegistrationJourneyType = RegistrationJourneyType.CsoSmallProducer;
        _registrationSumissionDetailsViewModel.ProducerDetails.NumberOfHoldingCompaniesClosedLoopRecycling = 2;
        _registrationSumissionDetailsViewModel.ProducerDetails.NumberOfSubsidiariesClosedLoopRecycling = 4;
        _registrationSumissionDetailsViewModel.CSOMembershipDetails =
        [
            new CsoMembershipDetailsDto
            {
                MemberId = "memberid1",
                MemberType = "small",
                NumberOfHoldingCompaniesClosedLoopRecycling = 2,
                NumberOfSubsidiariesClosedLoopRecycling = 4
            }
        ];
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .Callback<CompliancePaymentRequest>(r => capturedRequest = r)
            .ReturnsAsync((CompliancePaymentResponse?)null);

        // Act
        await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        capturedRequest.Should().NotBeNull();
        var members = capturedRequest!.ComplianceSchemeMembers!.ToList();
        members.Should().ContainSingle();
        members[0].NoOfHoldingCompaniesClosedLoopRecycling.Should().Be(0);
        members[0].NoOfSubsidiariesClosedLoopRecycling.Should().Be(0);
        members[0].IsClosedLoopRecycling.Should().BeFalse();
    }

    [TestMethod]
    public async Task InvokeAsync_WhenCsoMembershipDetailsEmpty_SendsNoComplianceSchemeMembers()
    {
        // Arrange
        CompliancePaymentRequest? capturedRequest = null;
        _registrationSumissionDetailsViewModel.NationCode = "GB-ENG";
        _registrationSumissionDetailsViewModel.RegistrationJourneyType = RegistrationJourneyType.CsoLargeProducer;
        _registrationSumissionDetailsViewModel.CSOMembershipDetails = [];
        _registrationSumissionDetailsViewModel.ProducerDetails = new ProducerDetailsDto
        {
            ProducerType = "large",
            NumberOfHoldingCompaniesClosedLoopRecycling = 1,
            NumberOfSubsidiariesClosedLoopRecycling = 8
        };
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .Callback<CompliancePaymentRequest>(r => capturedRequest = r)
            .ReturnsAsync((CompliancePaymentResponse?)null);

        // Act
        await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ComplianceSchemeMembers.Should().BeEmpty();
    }

    [TestMethod]
    public async Task InvokeAsync_WhenCsoMembersPresent_DoesNotUseProducerDetailsForClosedLoopCounts()
    {
        // Arrange
        CompliancePaymentRequest? capturedRequest = null;
        _registrationSumissionDetailsViewModel.RegistrationJourneyType = RegistrationJourneyType.CsoLargeProducer;
        _registrationSumissionDetailsViewModel.ProducerDetails.NumberOfHoldingCompaniesClosedLoopRecycling = 5;
        _registrationSumissionDetailsViewModel.ProducerDetails.NumberOfSubsidiariesClosedLoopRecycling = 7;
        _registrationSumissionDetailsViewModel.CSOMembershipDetails =
        [
            new CsoMembershipDetailsDto
            {
                MemberId = "memberid1",
                MemberType = "large",
                NumberOfHoldingCompaniesClosedLoopRecycling = null,
                NumberOfSubsidiariesClosedLoopRecycling = 0
            }
        ];
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .Callback<CompliancePaymentRequest>(r => capturedRequest = r)
            .ReturnsAsync((CompliancePaymentResponse?)null);

        // Act
        await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        capturedRequest.Should().NotBeNull();
        var members = capturedRequest!.ComplianceSchemeMembers!.ToList();
        members[0].NoOfSubsidiariesClosedLoopRecycling.Should().Be(0);
        members[0].NoOfHoldingCompaniesClosedLoopRecycling.Should().Be(0);
        members[0].IsClosedLoopRecycling.Should().BeFalse();
    }

    [TestMethod]
    public async Task InvokeAsync_SubsidiariesCompanyFee_Excludes_Omp_And_Clr_From_SubsidiariesFee()
    {
        // Arrange
        const decimal subsidiariesFeePence = 100_000m;
        const decimal ompPence = 20_000m;
        const decimal clrPence = 30_000m;
        _registrationSumissionDetailsViewModel.CSOMembershipDetails =
        [
            new CsoMembershipDetailsDto
            {
                MemberId = "memberid1",
                MemberType = "large",
                NumberOfSubsidiaries = 2
            }
        ];
        _registrationSumissionDetailsViewModel.SubmissionDetails = new SubmissionDetailsViewModel
        {
            TimeAndDateOfSubmission = DateTime.UtcNow.AddDays(-1)
        };
        _paymentFacadeServiceMock.Setup(x => x.GetCompliancePaymentDetailsAsync(It.IsAny<CompliancePaymentRequest>()))
            .ReturnsAsync(new CompliancePaymentResponse
            {
                ApplicationProcessingFee = 100.00M,
                TotalChargeableItems = 1000.00M,
                PreviousPaymentsReceived = 500.00M,
                TotalOutstanding = 500.00M,
                ComplianceSchemeMembers =
                [
                    new()
                    {
                        MemberId = "memberid1",
                        MemberType = "large",
                        MemberFee = 2.00M,
                        SubsidiaryFee = subsidiariesFeePence,
                        SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                        {
                            SubsidiaryOnlineMarketPlaceFee = ompPence,
                            TotalSubsidiariesClosedLoopRecyclingFees = clrPence,
                            CountOfClosedLoopRecyclingSubsidiaries = 1
                        }
                    }
                ]
            });

        // Act
        var result = await _sut.InvokeAsync(_registrationSumissionDetailsViewModel);

        // Assert
        var model = result.ViewData.Model as CompliancePaymentDetailsViewModel;
        model.Should().NotBeNull();
        model!.SubsidiariesCompanyCount.Should().Be(2);
        model.SubsidiariesCompanyFee.Should().Be((subsidiariesFeePence - ompPence - clrPence) / 100m);
        model.SubsidiariesClosedLoopRecyclingCount.Should().Be(1);
        model.SubsidiariesClosedLoopRecyclingFee.Should().Be(clrPence / 100m);
    }
}