using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class AccreditationStatusMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AccreditationStatusMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithAccreditationMaterialPaymentFees_ShouldReturnAccreditationStatusSession()
    {
        // Arrange
        var source = new AccreditationMaterialPaymentFees
        {
            RegistrationMaterialId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"),
            ApplicationReferenceNumber = "REF-001",
            FeeAmount = 150.50m,
            MaterialName = "Glass",
            OrganisationName = "Eco Recycling Ltd",
            SiteAddress = "456 Sustainable Way",
            SubmittedDate = new DateTime(2025, 4, 15),
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "Environment Agency"
        };

        // Act
        var result = _mapper.Map<AccreditationStatusSession>(source);

        // Assert
        result.Should().NotBeNull();
        result.ApplicationReferenceNumber.Should().Be(source.ApplicationReferenceNumber);
        result.FeeAmount.Should().Be(source.FeeAmount);
        result.MaterialName.Should().Be(source.MaterialName);
        result.OrganisationName.Should().Be(source.OrganisationName);
        result.SiteAddress.Should().Be(source.SiteAddress);
        result.SubmittedDate.Should().Be(source.SubmittedDate);
        result.RegistrationMaterialId.Should().Be(source.RegistrationMaterialId);
    }


    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnFeesDueViewModel()
    {
        // Arrange
        var session = CreateSession();

        // Act
        var result = _mapper.Map<FeesDueViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.FeeAmount.Should().Be(session.FeeAmount);
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnPaymentCheckViewModel()
    {
        // Arrange
        var session = CreateSession();

        // Act
        var result = _mapper.Map<PaymentCheckViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.FullPaymentMade.Should().Be(session.FullPaymentMade);
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnPaymentMethodViewModel()
    {
        // Arrange
        var session = CreateSession();

        // Act
        var result = _mapper.Map<PaymentMethodViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be(session.PaymentMethod);
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnPaymentDateViewModel_WithDate()
    {
        // Arrange
        var session = CreateSession();
        session.PaymentDate = new DateTime(2025, 5, 30);

        // Act
        var result = _mapper.Map<PaymentDateViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.Day.Should().Be(30);
        result.Month.Should().Be(5);
        result.Year.Should().Be(2025);
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnPaymentDateViewModel_WithoutDate()
    {
        // Arrange
        var session = CreateSession();
        session.PaymentDate = null;

        // Act
        var result = _mapper.Map<PaymentDateViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.Day.Should().BeNull();
        result.Month.Should().BeNull();
        result.Year.Should().BeNull();
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnPaymentReviewViewModel()
    {
        // Arrange
        var session = CreateSession();

        // Act
        var result = _mapper.Map<PaymentReviewViewModel>(session);

        // Assert
        result.Should().NotBeNull();
        result.PaymentDate.Should().Be(session.PaymentDate);
        result.PaymentMethod.Should().Be(session.PaymentMethod);
    }

    [TestMethod]
    public void Map_WhenCalledWithAccreditationStatusSession_ShouldReturnAccreditationOfflinePaymentRequest()
    {
        // Arrange
        var session = CreateSession();

        // Act
        var result = _mapper.Map<AccreditationOfflinePaymentRequest>(session);

        // Assert
        result.Should().NotBeNull();
        result.PaymentReference.Should().Be(session.ApplicationReferenceNumber);
        result.Amount.Should().Be(session.FeeAmount);
    }

    private static AccreditationStatusSession CreateSession()
    {
        return new AccreditationStatusSession
        {
            RegistrationId = Guid.NewGuid(),
            RegistrationMaterialId = Guid.NewGuid(),
            ApplicationReferenceNumber = "REF-001",
            FeeAmount = 150.50m,
            FullPaymentMade = true,
            PaymentMethod = PaymentMethodType.CreditOrDebitCard,
            PaymentDate = new DateTime(2025, 5, 30),
            MaterialName = "Glass",
            OrganisationName = "Recycling Co.",
            SiteAddress = "123 Green Road, Eco Town",
            SubmittedDate = new DateTime(2025, 5, 1)
        };
    }
}
