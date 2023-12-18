using EPR.RegulatorService.Frontend.Web.ViewModels.Shared.GovUK;
using EPR.RegulatorService.Frontend.Web;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels
{

    [TestClass]
    public class ErrorsViewModelTests
    {
        [TestMethod]
        public void Constructor_WithLocalizer_CreatesOrderedErrors()
        {
            // Arrange
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("Field1", new List<ErrorViewModel>()),
                ("Field2", new List<ErrorViewModel>()),
                ("Field3", new List<ErrorViewModel>()),
            };

            var localizer = new Mock<IStringLocalizer<SharedResources>>();
            var localizedString1 = new LocalizedString("Error1", "Localized Error 1");
            var localizedString2 = new LocalizedString("Error2", "Localized Error 2");
            localizer.Setup(x => x["Error1"]).Returns(localizedString1);
            localizer.Setup(x => x["Error2"]).Returns(localizedString2);

            // Act
            var viewModel = new ErrorsViewModel(errors, localizer.Object);

            // Assert
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(expected: 3, actual: viewModel.Errors.Count);
        }

        [TestMethod]
        public void Constructor_WithViewLocalizer_CreatesOrderedErrors()
        {
            // Arrange
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("Field1", new List<ErrorViewModel>()),
                ("Field2", new List<ErrorViewModel>()),
                ("Field3", new List<ErrorViewModel>()),
            };

            var localizer = new Mock<IViewLocalizer>();
            var localizedView1 = new LocalizedHtmlString("Error1", "Localized Error 1");
            var localizedView2 = new LocalizedHtmlString("Error2", "Localized Error 2");
            localizer.Setup(x => x["Error1"]).Returns(localizedView1);
            localizer.Setup(x => x["Error2"]).Returns(localizedView2);

            // Act
            var viewModel = new ErrorsViewModel(errors, localizer.Object);

            // Assert
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(expected: 3, actual: viewModel.Errors.Count);
        }

        [TestMethod]
        public void HasErrorKey_ReturnsTrueForExistingKey()
        {
            // Arrange
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("Field1", new List<ErrorViewModel>()),
                ("Field2", new List<ErrorViewModel>()),
                ("Field3", new List<ErrorViewModel>()),
            };

            var localizer = new Mock<IStringLocalizer<SharedResources>>();

            var viewModel = new ErrorsViewModel(errors, localizer.Object);

            // Act
            var hasField1 = viewModel.HasErrorKey("Field1");
            var hasField2 = viewModel.HasErrorKey("Field2");
            var hasField3 = viewModel.HasErrorKey("Field3");

            // Assert
            Assert.IsTrue(hasField1);
            Assert.IsTrue(hasField2);
            Assert.IsTrue(hasField3);
        }

        [TestMethod]
        public void HasErrorKey_ReturnsFalseForNonExistingKey()
        {
            // Arrange
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("Field1", new List<ErrorViewModel>()),
                ("Field2", new List<ErrorViewModel>()),
            };

            var localizer = new Mock<IStringLocalizer<SharedResources>>();

            var viewModel = new ErrorsViewModel(errors, localizer.Object);

            // Act
            var hasField3 = viewModel.HasErrorKey("Field3");

            // Assert
            Assert.IsFalse(hasField3);
        }
    }
}
