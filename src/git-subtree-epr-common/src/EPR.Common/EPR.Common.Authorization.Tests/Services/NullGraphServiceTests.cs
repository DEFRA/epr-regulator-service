using AutoFixture;
using EPR.Common.Authorization.Services;
using FluentAssertions;

namespace EPR.Common.Authorization.Test.Services;

[TestClass]
public class NullGraphServiceTests
{
    private readonly NullGraphService _systemUnderTest = new();
    private readonly IFixture _fixture = new Fixture();

    [TestMethod]
    public async Task PatchUserProperty_DoesNotThrowException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var propertyName = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        // Act
        var act = async () => await _systemUnderTest.PatchUserProperty(userId, propertyName, value);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task QueryUserPropertyUserProperty_ReturnsNull()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var propertyName = _fixture.Create<string>();

        // Act
        var result = await _systemUnderTest.QueryUserProperty(userId, propertyName);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Empty_Returns_Valid_New_Instance()
    {
        // Act / Assert
        var instance = NullGraphService.Empty;
        instance.Should().NotBeNull();
        ReferenceEquals(instance, _systemUnderTest).Should().BeFalse();
    }
}
