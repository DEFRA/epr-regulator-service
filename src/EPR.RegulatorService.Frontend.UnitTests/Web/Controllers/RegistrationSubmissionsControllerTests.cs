namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [TestClass]
    public class RegistrationSubmissionsControllerTests : RegistrationSubmissionsTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        #region RegistrationSubmissions

        #region Initialisation and Basic Session state
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
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 2;
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(2);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnsModel_WithPageNumber_1_When_Supplied_Null()
        {
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(1);
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber.Should().Be(1);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_3()
        {
            var result = await _controller.RegistrationSubmissions(3);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(3);
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber.Should().Be(3);
        }
        #endregion Initialisation and Basic Session state

        #region Session Models and Filter states between gets and posts
        #region Happy Path
        [TestMethod]
        public async Task RegistrationSubmissions_Initialises_ListViewModel_When_New()
        {

        }

        [TestMethod]
        public async Task RegistrationSubmissions_Retrieves_ListViewModel_After_Postback()
        {

        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_ListViewModel_When_Filters_Posted()
        {

        }

        [TestMethod]
        public async Task RegistrationSubmissions_Resets_PageNumber_When_Filters_Posted()
        {

        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_ListViewModel_And_Session_When_NewPage_Wanted()
        {

        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_Clears_ListViewModel_And_Session_Filters_When_ClearFilters_Recevied()
        {

        }
        #endregion Happy Path

        #region Sad Path
        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsEmpty()
        {
            var viewModel = new RegistrationSubmissionsFilterViewModel();
            var result = await _controller.RegistrationSubmissions(viewModel, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsInvalid()
        {
            var viewModel = new RegistrationSubmissionsFilterViewModel();
            var result = await _controller.RegistrationSubmissions(viewModel, "anything");
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsSubmitFilter_And_No_Filters_Supplied()
        {
            var result = await _controller.RegistrationSubmissions(null, FilterActions.SubmitFilters);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_No_Filter_Or_Query_Supplied()
        {
            var result = await _controller.RegistrationSubmissions(null, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task PostingTo_RegistrationSubmissions_Return_ErrorPage_When_Exception_Received()
        {
            MockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(null, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToPageResult>();
            (result as RedirectToPageResult).PageName.Should().Be(PagePath.Error);
        }

        [TestMethod]
        public async Task GettingFrom_RegistrationSubmissions_Return_ErrorPage_When_Exception_Received()
        {
            MockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(1);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToPageResult>();
            (result as RedirectToPageResult).PageName.Should().Be(PagePath.Error);
        }

        [TestMethod]
        public async Task PostTo_RegistrationSubmissions_Logs_Error_When_Exception_Received()
        {
            MockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(null,null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToPageResult>();
            (result as RedirectToPageResult).PageName.Should().Be(PagePath.Error);
            _loggerMock.Verify(
                        x => x.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received processing POST to RegistrationSubmissionsController.RegistrationSubmissions")),
                            It.Is<Exception>((v, t)=> v.ToString().Contains("Test")),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
        }

        [TestMethod]
        public async Task GettingFrom_RegistrationSubmissions_Logs_Error_When_Exception_Received()
        {
            MockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(1);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToPageResult>();
            (result as RedirectToPageResult).PageName.Should().Be(PagePath.Error);
            _loggerMock.Verify(
                        x => x.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received processing GET to RegistrationSubmissionsController.RegistrationSubmissions")),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
        }
        #endregion Sad Path

        #endregion Session Models and Filter states between gets and posts
        #endregion RegistrationSubmissions

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
                }
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

        #endregion
    }
}