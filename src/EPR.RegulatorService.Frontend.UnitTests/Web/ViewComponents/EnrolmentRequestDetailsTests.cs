using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Web.Constants;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents
{
    [TestClass]
    public class EnrolmentRequestDetailsTests : ViewComponentsTestBase
    {
        private EnrolmentRequestDetailsViewComponent _component;
        private OrganisationEnrolments _organisationEnrolments;
        private User _approvedUser;
        private List<User> _delegatedUsers;

        [TestInitialize]
        public void Setup()
        {
            _component = new EnrolmentRequestDetailsViewComponent();
            _organisationEnrolments = MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails();
            _approvedUser = _organisationEnrolments.Users.Find(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson)!;
            _delegatedUsers = _organisationEnrolments.Users.Where(x => x.Enrolment.ServiceRole == ServiceRole.DelegatedPerson).ToList();
        } 

        [TestMethod]
        public void Invoke_SetsModel()
        {
            // Act
            var model = _component.InvokeAsync(
                false, _approvedUser, _delegatedUsers).Result.ViewData?.Model as EnrolmentRequestDetailsModel;
            
            // Assert
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.ApprovedUser);
            Assert.AreEqual(expected: 2, actual: model.DelegatedUsers.Count);
            Assert.IsFalse(model.IsApprovedUserAccepted);
        }
    }
}

