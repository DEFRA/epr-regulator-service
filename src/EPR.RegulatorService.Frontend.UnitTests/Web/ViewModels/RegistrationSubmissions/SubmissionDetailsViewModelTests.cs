namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class SubmissionDetailsViewModelTests
    {
        private Guid _testFileId;
        private DateTime _decisionDate;
        private DateTime _submissionDate;
        private DateTime _registrationDate;
        private DateTime _statusPendingDate;

        private SubmissionDetailsViewModel.FileDetails _testFileDetailsCompany;
        private SubmissionDetailsViewModel.FileDetails _testFileDetailsPartnership;
        private SubmissionDetailsViewModel.FileDetails _testFileDetailsBrands;
        private RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails _testDomainFileDetailsCompany;
        private RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails _testDomainFileDetailsPartnership;
        private RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails _testDomainFileDetailsBrands;

        [TestInitialize]
        public void Setup()
        {
            _testFileId = Guid.NewGuid();
            _decisionDate = new DateTime(2023, 12, 1, 14, 30, 0, DateTimeKind.Unspecified);
            _submissionDate = new DateTime(2023, 12, 5, 16, 45, 0, DateTimeKind.Unspecified);
            _registrationDate = new DateTime(2024, 12, 5, 16, 45, 0, DateTimeKind.Unspecified);
            _statusPendingDate = new DateTime(2023, 12, 27, 12, 15, 0, DateTimeKind.Unspecified);

            _testFileDetailsCompany = new SubmissionDetailsViewModel.FileDetails
            {
                Type = FileType.company,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };

            _testDomainFileDetailsCompany = new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
            {
                Type = FileType.company,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };

            _testFileDetailsPartnership = new SubmissionDetailsViewModel.FileDetails
            {
                Type = FileType.partnership,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };

            _testDomainFileDetailsPartnership = new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
            {
                Type = FileType.partnership,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };

            _testFileDetailsBrands = new SubmissionDetailsViewModel.FileDetails
            {
                Type = FileType.brands,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };

            _testDomainFileDetailsBrands = new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
            {
                Type = FileType.brands,
                FileName = "TestFile.pdf",
                FileId = _testFileId,
                BlobName = "test/blob/name"
            };
        }

        [TestMethod]
        public void FileDetails_ImplicitOperator_ToViewModel_ShouldMapCorrectlyCompany()
        {
            // Act
            SubmissionDetailsViewModel.FileDetails result = _testDomainFileDetailsCompany;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testDomainFileDetailsCompany.Type, result.Type);
            Assert.AreEqual(_testDomainFileDetailsCompany.FileName, result.FileName);
            Assert.AreEqual(_testDomainFileDetailsCompany.FileId, result.FileId);
            Assert.AreEqual(_testDomainFileDetailsCompany.BlobName, result.BlobName);
            Assert.AreEqual(FileDownloadTypes.OrganisationDetails, result.DownloadType);
            Assert.AreEqual("SubmissionDetails.OrganisationDetails", result.Label);
        }

        [TestMethod]
        public void FileDetails_ImplicitOperator_ToViewModel_ShouldMapCorrectlyPartnership()
        {
            // Act
            SubmissionDetailsViewModel.FileDetails result = _testDomainFileDetailsPartnership;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testDomainFileDetailsPartnership.Type, result.Type);
            Assert.AreEqual(_testDomainFileDetailsPartnership.FileName, result.FileName);
            Assert.AreEqual(_testDomainFileDetailsPartnership.FileId, result.FileId);
            Assert.AreEqual(_testDomainFileDetailsPartnership.BlobName, result.BlobName);
            Assert.AreEqual(FileDownloadTypes.PartnershipDetails, result.DownloadType);
            Assert.AreEqual("SubmissionDetails.PartnerDetails", result.Label);
        }

        [TestMethod]
        public void FileDetails_ImplicitOperator_ToViewModel_ShouldMapCorrectlyBrands()
        {
            // Act
            SubmissionDetailsViewModel.FileDetails result = _testDomainFileDetailsBrands;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testDomainFileDetailsBrands.Type, result.Type);
            Assert.AreEqual(_testDomainFileDetailsBrands.FileName, result.FileName);
            Assert.AreEqual(_testDomainFileDetailsBrands.FileId, result.FileId);
            Assert.AreEqual(_testDomainFileDetailsBrands.BlobName, result.BlobName);
            Assert.AreEqual(FileDownloadTypes.BrandDetails, result.DownloadType);
            Assert.AreEqual("SubmissionDetails.BrandDetails", result.Label);
        }

        [TestMethod]
        public void FileDetails_ImplicitOperator_FromViewModel_ShouldMapCorrectly()
        {
            // Act
            RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails result = _testFileDetailsCompany;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testFileDetailsCompany.Type, result.Type);
            Assert.AreEqual(_testFileDetailsCompany.FileName, result.FileName);
            Assert.AreEqual(_testFileDetailsCompany.FileId, result.FileId);
            Assert.AreEqual(_testFileDetailsCompany.BlobName, result.BlobName);
        }

        [TestMethod]
        [DataRow(RegistrationSubmissionStatus.Refused)]
        [DataRow(RegistrationSubmissionStatus.Queried)]
        [DataRow(RegistrationSubmissionStatus.Updated)]
        [DataRow(RegistrationSubmissionStatus.Accepted)]
        [DataRow(RegistrationSubmissionStatus.Rejected)]
        public void DisplayAppropriateSubmissionDate_ShouldReturnFormattedDate_BasedOnStatusNotCancelled(
            RegistrationSubmissionStatus status)
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                Status = status,
                LatestDecisionDate = _decisionDate,
                TimeAndDateOfSubmission = _submissionDate
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("01 December 2023 14:30:00", result);
        }

        [TestMethod]
        [DataRow(RegistrationSubmissionStatus.Granted)]
        public void DisplayAppropriateSubmissionDate_ShouldReturnFormattedDate_BasedOnStatusGranted(
            RegistrationSubmissionStatus status)
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                Status = status,
                LatestDecisionDate = _decisionDate,
                TimeAndDateOfSubmission = _submissionDate,
                RegistrationDate = _registrationDate,
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("05 December 2024 16:45:00", result);
        }

        [TestMethod]
        public void DisplayAppropriateSubmissionDate_ShouldReturnFormattedDate_BasedOnEmptyStatus()
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                StatusPendingDate = _statusPendingDate,
                TimeAndDateOfSubmission = _submissionDate
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("05 December 2023 16:45:00", result);
        }

        [TestMethod]
        public void DisplayAppropriateSubmissionDate_ShouldReturnFormattedDate_BasedOnCancelledStatus_WhenThereIsAStatusPendingDate()
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Cancelled,
                StatusPendingDate = _statusPendingDate,
                TimeAndDateOfSubmission = _submissionDate
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("27 December 2023", result);
        }

        [TestMethod]
        public void DisplayAppropriateSubmissionDate_ShouldReturnFormattedDate_BasedOnCancelledStatus_WhenThereIsNotAStatusPendingDate()
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Cancelled,
                LatestDecisionDate = _decisionDate,
                TimeAndDateOfSubmission = _submissionDate
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("01 December 2023 14:30:00", result);
        }

        [TestMethod]
        public void DisplayAppropriateSubmissionDate_ShouldFallbackToSubmissionDate_WhenDecisionDateIsNull()
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Pending,
                LatestDecisionDate = null,
                TimeAndDateOfSubmission = _submissionDate
            };

            // Act
            string result = viewModel.DisplayAppropriateSubmissionDate();

            // Assert
            Assert.AreEqual("05 December 2023 16:45:00", result);
        }

        [TestMethod]
        public void ImplicitOperator_ToDomain_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var viewModel = new SubmissionDetailsViewModel
            {
                AccountRoleId = 1,
                Telephone = "1234567890",
                Email = "test@example.com",
                DeclaredBy = "John Doe",
                SubmittedBy = "Jane Smith",
                LatestDecisionDate = _decisionDate,
                Status = RegistrationSubmissionStatus.Granted,
                SubmittedOnTime = true,
                TimeAndDateOfSubmission = _submissionDate,
                Files = [_testFileDetailsCompany]
            };

            // Act
            RegistrationSubmissionOrganisationSubmissionSummaryDetails result = viewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(viewModel.AccountRoleId, result.AccountRoleId);
            Assert.AreEqual(viewModel.Telephone, result.Telephone);
            Assert.AreEqual(viewModel.Email, result.Email);
            Assert.AreEqual(viewModel.DeclaredBy, result.DeclaredBy);
            Assert.AreEqual(viewModel.SubmittedBy, result.SubmittedBy);
            Assert.AreEqual(viewModel.LatestDecisionDate, result.DecisionDate);
            Assert.AreEqual(viewModel.Status, result.Status);
            Assert.AreEqual(viewModel.SubmittedOnTime, result.SubmittedOnTime);
            Assert.AreEqual(viewModel.TimeAndDateOfSubmission, result.TimeAndDateOfSubmission);
            Assert.AreEqual(viewModel.Files.Count, result.Files.Count);
            Assert.AreEqual(_testFileDetailsCompany.Type, result.Files[0].Type);
        }

        [TestMethod]
        public void ImplicitOperator_ToDomain_ShouldMapAllPropertiesCorrectlyNullSubmissionDetails()
        {
            // Arrange
            var viewModel = (SubmissionDetailsViewModel)null;

            // Act
            RegistrationSubmissionOrganisationSubmissionSummaryDetails result = viewModel;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Constructor_ShouldInitializeFilesAsEmptyList()
        {
            // Act
            var viewModel = new SubmissionDetailsViewModel();

            // Assert
            Assert.IsNotNull(viewModel.Files);
            Assert.AreEqual(0, viewModel.Files.Count);
        }
    }
}
