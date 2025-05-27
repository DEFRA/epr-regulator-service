namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

using EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc.ViewComponents;

[TestClass]
public class SubmissionDetailsViewComponentTests : ViewComponentsTestBase
{
    [TestMethod]
    public async Task InvokeAsync_ReturnsCorrectViewAndModel()
    {
        // Arrange
        var submissionDetailsViewModel = (new Fixture()).Build<SubmissionDetailsViewModel>()
            .Create();
        var viewComponent = new SubmissionDetailsViewComponent();

        // Act
        var result = await viewComponent.InvokeAsync(submissionDetailsViewModel, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull().And.BeOfType<ViewViewComponentResult>();
        Assert.IsNotNull(result.ViewData);
        var (model, submissionId) = result.ViewData.Model.As<(SubmissionDetailsViewModel, Guid)>();
        model.Should().NotBeNull();
        model.Should().BeOfType<SubmissionDetailsViewModel>();
        submissionId.Should().NotBeEmpty();
    }
}