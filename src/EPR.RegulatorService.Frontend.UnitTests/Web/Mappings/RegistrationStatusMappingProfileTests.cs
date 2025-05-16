using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class RegistrationStatusMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<RegistrationStatusMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithModel_ShouldReturnSessionEntity()
    {
        // Arrange
        var registrationMaterialPaymentFees = new RegistrationMaterialPaymentFees
        {
            RegistrationId = 123,
            OrganisationName = "Test Org Name",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            SiteAddress = "Test Site Address",
            RegistrationMaterialId = 1234,
            MaterialName = "Plastic",
            FeeAmount = 2921,
            ApplicationReferenceNumber = "ABC123456",
            SubmittedDate = DateTime.Now.AddDays(-7),
            Regulator = "GB-ENG"
        };

        // Act
        var registrationStatusSession = _mapper.Map<RegistrationStatusSession>(registrationMaterialPaymentFees);

        // Assert
        registrationStatusSession.Should().NotBeNull();
        registrationStatusSession.RegistrationId.Should().Be(registrationMaterialPaymentFees.RegistrationId);
        registrationStatusSession.OrganisationName.Should().Be(registrationMaterialPaymentFees.OrganisationName);
        registrationStatusSession.ApplicationType.Should().Be(registrationMaterialPaymentFees.ApplicationType);
        registrationStatusSession.SiteAddress.Should().Be(registrationMaterialPaymentFees.SiteAddress);
        registrationStatusSession.RegistrationMaterialId.Should().Be(registrationMaterialPaymentFees.RegistrationMaterialId);
        registrationStatusSession.MaterialName.Should().Be(registrationMaterialPaymentFees.MaterialName);
        registrationStatusSession.FeeAmount.Should().Be(registrationMaterialPaymentFees.FeeAmount);
        registrationStatusSession.ApplicationReferenceNumber.Should().Be(registrationMaterialPaymentFees.ApplicationReferenceNumber);
        registrationStatusSession.SubmittedDate.Should().Be(registrationMaterialPaymentFees.SubmittedDate);
        registrationStatusSession.Regulator.Should().Be(registrationMaterialPaymentFees.Regulator);
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnFeesDueViewModel()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<FeesDueViewModel>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        viewModel.SiteAddress.Should().Be(registrationStatusSession.SiteAddress);
        viewModel.ApplicationType.Should().Be(registrationStatusSession.ApplicationType);
        viewModel.ApplicationReferenceNumber.Should().Be(registrationStatusSession.ApplicationReferenceNumber);
        viewModel.RegistrationMaterialId.Should().Be(registrationStatusSession.RegistrationMaterialId);
        viewModel.MaterialName.Should().Be(registrationStatusSession.MaterialName);
        viewModel.SubmittedDate.Should().Be(registrationStatusSession.SubmittedDate);
        viewModel.FeeAmount.Should().Be(registrationStatusSession.FeeAmount);
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnPaymentCheckViewModel()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<PaymentCheckViewModel>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        viewModel.SiteAddress.Should().Be(registrationStatusSession.SiteAddress);
        viewModel.ApplicationType.Should().Be(registrationStatusSession.ApplicationType);
        viewModel.FeeAmount.Should().Be(registrationStatusSession.FeeAmount);
        viewModel.FullPaymentMade.Should().BeNull();
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnPaymentMethodViewModel()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<PaymentMethodViewModel>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        viewModel.SiteAddress.Should().Be(registrationStatusSession.SiteAddress);
        viewModel.ApplicationType.Should().Be(registrationStatusSession.ApplicationType);
        viewModel.PaymentMethod.Should().Be(registrationStatusSession.PaymentMethod);
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnPaymentDateViewModel()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<PaymentDateViewModel>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        viewModel.SiteAddress.Should().Be(registrationStatusSession.SiteAddress);
        viewModel.ApplicationType.Should().Be(registrationStatusSession.ApplicationType);
        viewModel.Day.Should().Be(registrationStatusSession.PaymentDate!.Value.Day);
        viewModel.Month.Should().Be(registrationStatusSession.PaymentDate!.Value.Month);
        viewModel.Year.Should().Be(registrationStatusSession.PaymentDate!.Value.Year);
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnPaymentReviewViewModel()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<PaymentReviewViewModel>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.MaterialName.Should().Be(registrationStatusSession.MaterialName);
        viewModel.SubmittedDate.Should().Be(registrationStatusSession.SubmittedDate);
        viewModel.PaymentMethod.Should().Be(registrationStatusSession.PaymentMethod);
        viewModel.PaymentDate.Should().Be(registrationStatusSession.PaymentDate);
        viewModel.DeterminationDate.Should().Be(DateTime.MinValue);
        viewModel.DeterminationWeeks.Should().Be(0);
    }

    [TestMethod]
    public void Map_WhenCalledWithSession_ShouldReturnOfflinePaymentRequest()
    {
        // Arrange
        var registrationStatusSession = CreateRegistrationStatusSession();

        // Act
        var viewModel = _mapper.Map<OfflinePaymentRequest>(registrationStatusSession);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Amount.Should().Be(registrationStatusSession.FeeAmount);
        viewModel.PaymentReference.Should().Be(registrationStatusSession.ApplicationReferenceNumber);
        viewModel.PaymentDate.Should().Be(registrationStatusSession.PaymentDate);
        viewModel.PaymentMethod.Should().Be(registrationStatusSession.PaymentMethod);
        viewModel.Regulator.Should().Be(registrationStatusSession.Regulator);
    }

    private static RegistrationStatusSession CreateRegistrationStatusSession() => new()
    {
        RegistrationId = 123,
        OrganisationName = "Test Org Name",
        ApplicationType = ApplicationOrganisationType.Reprocessor,
        SiteAddress = "Test Site Address",
        RegistrationMaterialId = 1234,
        MaterialName = "Plastic",
        FeeAmount = 2921,
        ApplicationReferenceNumber = "ABC123456",
        SubmittedDate = DateTime.Now.AddDays(-7),
        Regulator = "GB-ENG",
        PaymentDate = DateTime.Now.AddDays(-14)
    };
}