namespace EPR.RegulatorService.Frontend.UnitTests.Core.Models.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

    [TestClass]
    public class RegistrationSubmissionsOrganisationPaymentDetailsTests
    {
        [TestMethod]
        public void RegistrationSubmissionsOrganisationPaymentDetails_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 120.00m,
                OfflinePaymentAmount = 50.00m
            };

            // Act & Assert
            Assert.AreEqual(100.00m, paymentDetails.ApplicationProcessingFee);
            Assert.AreEqual(50.00m, paymentDetails.OnlineMarketplaceFee);
            Assert.AreEqual(30.00m, paymentDetails.SubsidiaryFee);
            Assert.AreEqual(120.00m, paymentDetails.PreviousPaymentsReceived);
            Assert.AreEqual(50.00m, paymentDetails.OfflinePaymentAmount);
        }

        [TestMethod]
        public void TotalChargeableItems_ShouldReturnCorrectSum()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m
            };

            // Act
            var totalChargeableItems = paymentDetails.TotalChargeableItems;

            // Assert
            Assert.AreEqual(180.00m, totalChargeableItems); // 100 + 50 + 30 = 180
        }

        [TestMethod]
        public void TotalOutstanding_ShouldReturnCorrectDifference()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 120.00m
            };

            // Act
            var totalOutstanding = paymentDetails.TotalOutstanding;

            // Assert
            Assert.AreEqual(60.00m, totalOutstanding); // 180 (TotalChargeableItems) - 120 (PreviousPaymentsReceived) = 60
        }

        [TestMethod]
        public void TotalOutstanding_ShouldBeZero_WhenPreviousPaymentsReceivedEqualsTotalChargeableItems()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 180.00m
            };

            // Act
            var totalOutstanding = paymentDetails.TotalOutstanding;

            // Assert
            Assert.AreEqual(0.00m, totalOutstanding); // 180 (TotalChargeableItems) - 180 (PreviousPaymentsReceived) = 0
        }

        [TestMethod]
        public void TotalOutstanding_ShouldBeNegative_WhenPreviousPaymentsReceivedExceedsTotalChargeableItems()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 200.00m
            };

            // Act
            var totalOutstanding = paymentDetails.TotalOutstanding;

            // Assert
            Assert.AreEqual(-20.00m, totalOutstanding); // 180 (TotalChargeableItems) - 200 (PreviousPaymentsReceived) = -20
        }

        [TestMethod]
        public void OfflinePaymentAmount_ShouldBeNullable()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 120.00m,
                OfflinePaymentAmount = null
            };

            // Act & Assert
            Assert.IsNull(paymentDetails.OfflinePaymentAmount);
        }

        [TestMethod]
        public void OfflinePaymentAmount_ShouldBeAssignedCorrectly()
        {
            // Arrange
            var paymentDetails = new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = 100.00m,
                OnlineMarketplaceFee = 50.00m,
                SubsidiaryFee = 30.00m,
                PreviousPaymentsReceived = 120.00m,
                OfflinePaymentAmount = 50.00m
            };

            // Act & Assert
            Assert.AreEqual(50.00m, paymentDetails.OfflinePaymentAmount);
        }
    }
}
