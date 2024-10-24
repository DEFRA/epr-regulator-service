namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Sessions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [TestClass]
    public class RegistrationSubmissionsControllerTests : RegistrationSubmissionsTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        #region RegistrationSubmissions

        [TestMethod]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel()
        {
            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel();
            string expectedBackLink = "/regulators/home";

            // Act
            var result = await _controller.RegistrationSubmissions(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult.Model.GetType());

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_FromSession_When_Supplied_Null()
        {
            _journeySession.RegulatorSession.CurrentPageNumber = 2;
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.PageNumber.Should().Be(2);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnsModel_WithPageNumber_1_When_Supplied_Null()
        {
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.PageNumber.Should().Be(1);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_3()
        {
            var result = await _controller.RegistrationSubmissions(3);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.PageNumber.Should().Be(3);
            _journeySession.RegulatorSession.CurrentPageNumber.Should().Be(3);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_ShouldHandleNullSession()
        {
            // Arrange
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync((JourneySession)null);
            int pageNumber = 1;

            // Act
            var result = await _controller.RegistrationSubmissions(pageNumber);

            // Assert
            _mockSessionManager.Verify(sm => sm.GetSessionAsync(It.IsAny<ISession>()), Times.Once);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as RegistrationSubmissionsViewModel;
            Assert.IsNotNull(model);

            // Since session was null, it should use default page number logic
            Assert.AreEqual(pageNumber, model.PageNumber);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_ShouldCreateANewJourneySession_WhenSessionManagerNull()
        {
            // Arrange
            var sut = new RegistrationSubmissionsController(
                null,
                _mockConfiguration.Object,
                _mockUrlsOptions.Object
                );

            // Act
            var result = sut.SessionManager;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JourneySessionManager));

            sut.Dispose();
        }

        #endregion

        #region QueryRegistrationSubmission

        [TestMethod]
        public async Task QueryRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange
            string expectedBacktoAllSubmissionsUrl = PagePath.RegistrationSubmissions;

            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                BackToAllSubmissionsUrl = expectedBacktoAllSubmissionsUrl
            };

            // Act
            var result = await _controller.QueryRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(QueryRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as QueryRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.Query, resultViewModel.Query);
            Assert.AreEqual(expectedViewModel.BackToAllSubmissionsUrl, resultViewModel.BackToAllSubmissionsUrl);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_SetsCorrectBackLinkInViewData()
        {
            // Act
            var result = await _controller.QueryRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel();
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenQueryIsValid()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = "Valid query within 400 characters." // Valid input
            };

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = new string('A', 401) // 401 characters
            };

            // Add a validation rule that the maximum length is 400 for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.Query), "Reason for querying application must be 400 characters or less");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)].Errors.Count > 0);
            Assert.AreEqual("Reason for querying application must be 400 characters or less",
                _controller.ModelState[nameof(model.Query)].Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenNoQueryIsProvided()
        {
            // Arrange
            var model = new QueryRegistrationSubmissionViewModel
            {
                Query = null // No query provided
            };

            // Simulate the model validation error for required Query field
            _controller.ModelState.AddModelError(nameof(model.Query), "Enter the reason you are querying this registration application");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)].Errors.Count > 0);
            Assert.AreEqual("Enter the reason you are querying this registration application",
                _controller.ModelState[nameof(model.Query)].Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        #endregion

        #region RejectRegistrationSubmission

        [TestMethod]
        public async Task RejectRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange
            string expectedBacktoAllSubmissionsUrl = PagePath.RegistrationSubmissions;

            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                BackToAllSubmissionsUrl = expectedBacktoAllSubmissionsUrl
            };

            // Act
            var result = await _controller.RejectRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(RejectRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as RejectRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.RejectReason, resultViewModel.RejectReason);
            Assert.AreEqual(expectedViewModel.BackToAllSubmissionsUrl, resultViewModel.BackToAllSubmissionsUrl);
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_SetsCorrectBackLinkInViewData()
        {
            // Act
            var result = await _controller.RejectRegistrationSubmission() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel();
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenRejectionReasonIsValid()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = "Valid rejection reason within 400 characters." // Valid input
            };

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = new string('A', 401) // 401 characters
            };

            // Add a validation rule that the maximum length is 400 for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Reason for rejecting application must be 400 characters or less");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)].Errors.Count > 0);
            Assert.AreEqual("Reason for rejecting application must be 400 characters or less",
                _controller.ModelState[nameof(model.RejectReason)].Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenNoRejectionReasonIsProvided()
        {
            // Arrange
            var model = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = null // No rejection reason provided
            };

            // Simulate the model validation error for required RejectionReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Enter the reason you are rejecting this registration application");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.AreEqual(model, result.Model);

            // Verify the error is correctly added to the model state
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)].Errors.Count > 0);
            Assert.AreEqual("Enter the reason you are rejecting this registration application",
                _controller.ModelState[nameof(model.RejectReason)].Errors[0].ErrorMessage);

            // Verify the back link is set correctly
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        #endregion

        #region RegistrationSubmissionDetails

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsViewResult()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsCorrectViewModel_ForValidOrganisationId()
        {
            // Arrange
            var organisationId = Guid.NewGuid(); // Simulate a valid Organisation ID
            string expectedViewName = nameof(_controller.RegistrationSubmissionDetails);
            var expectedViewModel = new RegistrationSubmissionDetailsViewModel
            {
                OrganisationId = organisationId,
                OrganisationReference = "215 148",
                OrganisationName = "Acme org Ltd.",
                RegistrationReferenceNumber = "REF001",
                ApplicationReferenceNumber = "REF002",
                OrganisationType = RegistrationSubmissionOrganisationType.large,
                BusinessAddress = new BusinessAddress
                {
                    BuildingName = string.Empty,
                    BuildingNumber = "10",
                    Street = "High Street",
                    County = "Randomshire",
                    PostCode = "A12 3BC"
                },
                CompaniesHouseNumber = "0123456",
                RegisteredNation = "Scotland",
                PowerBiLogin = "https://app.powerbi.com/",
                Status = RegistrationSubmissionStatus.queried,
                SubmissionDetails = new SubmissionDetailsViewModel
                {
                    Status = RegistrationSubmissionStatus.queried,
                    DecisionDate = new DateTime(2024, 10, 21, 16, 23, 42, DateTimeKind.Utc),
                    TimeAndDateOfSubmission = new DateTime(2024, 7, 10, 16, 23, 42, DateTimeKind.Utc),
                    SubmittedOnTime = true,
                    SubmittedBy = "Sally Smith",
                    AccountRole = Frontend.Core.Enums.ServiceRole.ApprovedPerson,
                    Telephone = "07553 937 831",
                    Email = "sally.smith@email.com",
                    DeclaredBy = "Sally Smith",
                    Files =
                    [
                        new() { Label = "SubmissionDetails.OrganisationDetails", FileName = "org.details.acme.csv", DownloadUrl = "#" },
                        new() { Label = "SubmissionDetails.BrandDetails", FileName = "brand.details.acme.csv", DownloadUrl = "#" },
                        new() { Label = "SubmissionDetails.PartnerDetails", FileName = "partner.details.acme.csv", DownloadUrl = "#" }
                    ]
                },
                PaymentDetails = new PaymentDetailsViewModel
                {
                    ApplicationProcessingFee = 134522.56M,
                    OnlineMarketplaceFee = 2534534.23M,
                    SubsidiaryFee = 1.34M,
                    PreviousPaymentsReceived = 20M
                },
                ProducerComments = "producer comment",
                RegulatorComments = "regulator comment"
                },
                BackToAllSubmissionsUrl = "/regulators/manage-registration-submissions"
            };

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedViewName, result.ViewName);
            var model = result.Model as RegistrationSubmissionDetailsViewModel;
            Assert.IsNotNull(model);

            // Assert model properties
            Assert.AreEqual(expectedViewModel.OrganisationId, model.OrganisationId);
            Assert.AreEqual(expectedViewModel.OrganisationReference, model.OrganisationReference);
            Assert.AreEqual(expectedViewModel.OrganisationName, model.OrganisationName);
            Assert.AreEqual(expectedViewModel.ApplicationReferenceNumber, model.ApplicationReferenceNumber);
            Assert.AreEqual(expectedViewModel.RegistrationReferenceNumber, model.RegistrationReferenceNumber);
            Assert.AreEqual(expectedViewModel.OrganisationType, model.OrganisationType);
            Assert.AreEqual(expectedViewModel.BackToAllSubmissionsUrl, model.BackToAllSubmissionsUrl);

            // Assert SubmissionDetailsViewModel properties
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Status, model.SubmissionDetails.Status);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.DecisionDate, model.SubmissionDetails.DecisionDate);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.TimeAndDateOfSubmission, model.SubmissionDetails.TimeAndDateOfSubmission);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedOnTime, model.SubmissionDetails.SubmittedOnTime);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedBy, model.SubmissionDetails.SubmittedBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.AccountRole, model.SubmissionDetails.AccountRole);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Telephone, model.SubmissionDetails.Telephone);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Email, model.SubmissionDetails.Email);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.DeclaredBy, model.SubmissionDetails.DeclaredBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Files.Count, model.SubmissionDetails.Files.Count);

            // Assert PaymentDetailsViewModel properties
            Assert.AreEqual(expectedViewModel.PaymentDetails.ApplicationProcessingFee, model.PaymentDetails.ApplicationProcessingFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.OnlineMarketplaceFee, model.PaymentDetails.OnlineMarketplaceFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.SubsidiaryFee, model.PaymentDetails.SubsidiaryFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.TotalChargeableItems, model.PaymentDetails.TotalChargeableItems);
            Assert.AreEqual(expectedViewModel.PaymentDetails.PreviousPaymentsReceived, model.PaymentDetails.PreviousPaymentsReceived);
            Assert.AreEqual(expectedViewModel.PaymentDetails.TotalOutstanding, model.PaymentDetails.TotalOutstanding);

            // Assert business address
            Assert.AreEqual(expectedViewModel.BusinessAddress.BuildingName, model.BusinessAddress.BuildingName);
            Assert.AreEqual(expectedViewModel.BusinessAddress.BuildingNumber, model.BusinessAddress.BuildingNumber);
            Assert.AreEqual(expectedViewModel.BusinessAddress.Street, model.BusinessAddress.Street);
            Assert.AreEqual(expectedViewModel.BusinessAddress.County, model.BusinessAddress.County);
            Assert.AreEqual(expectedViewModel.BusinessAddress.PostCode, model.BusinessAddress.PostCode);

            Assert.AreEqual(expectedViewModel.CompaniesHouseNumber, model.CompaniesHouseNumber);
            Assert.AreEqual(expectedViewModel.RegisteredNation, model.RegisteredNation);
            Assert.AreEqual(expectedViewModel.PowerBiLogin, model.PowerBiLogin);
            Assert.AreEqual(expectedViewModel.Status, model.Status);

            Assert.AreEqual(expectedViewModel.ProducerComments, model.ProducerComments);
            Assert.AreEqual(expectedViewModel.RegulatorComments, model.RegulatorComments);
        } 

        [TestMethod]
        public async Task RegistrationSubmissionDetails_SetsCorrectBackLink()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissions}");
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_RedirectsToRegistrationSubmissions_WhenCalled()
        {
            // Arrange
            var model = new PaymentDetailsViewModel();

            // Act
            var result = await _controller.SubmitOfflinePayment(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PagePath.RegistrationSubmissions, result.Url);
        }

        #endregion
    }
}