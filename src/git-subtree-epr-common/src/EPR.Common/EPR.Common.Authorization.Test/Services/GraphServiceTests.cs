using EPR.Common.Authorization.Config;
using EPR.Common.Authorization.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using System.Text.Json;

namespace EPR.Common.Authorization.Test.Services;

[TestClass]
public class GraphServiceTests
{
    private const string ExtensionClientId = "a12-b34";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Mock<IOptions<AzureB2CExtensionConfig>> _azureB2CExtensionOptions;
    private Mock<GraphServiceClient> _graphServiceClientMock;
    private Mock<ILogger<GraphService>> _loggerMock;
    private Mock<IRequestAdapter> _requestAdapterMock;
    private Mock<ISerializationWriterFactory> _serializationWriterFactoryMock;
    private GraphService _systemUnderTest;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [TestInitialize]
    public void SetUp()
    {
        _requestAdapterMock = new Mock<IRequestAdapter>();

        _serializationWriterFactoryMock = new Mock<ISerializationWriterFactory>();
        _serializationWriterFactoryMock.Setup(factory => factory.GetSerializationWriter(It.IsAny<string>()))
            .Returns((string _) => new JsonSerializationWriter());

        _requestAdapterMock.SetupGet(adapter => adapter.BaseUrl).Returns("http://graph.test.internal/mock");
        _requestAdapterMock.SetupSet(adapter => adapter.BaseUrl = It.IsAny<string>());
        _requestAdapterMock.Setup(adapter => adapter.EnableBackingStore(It.IsAny<IBackingStoreFactory>()));
        _requestAdapterMock.SetupGet(adapter => adapter.SerializationWriterFactory).Returns(_serializationWriterFactoryMock.Object);

        _graphServiceClientMock = new Mock<GraphServiceClient>(
            _requestAdapterMock.Object,
            It.IsAny<string>());

        _azureB2CExtensionOptions = new Mock<IOptions<AzureB2CExtensionConfig>>();
        _azureB2CExtensionOptions!
            .Setup(x => x.Value)
            .Returns(new AzureB2CExtensionConfig
            {
                ExtensionsClientId = ExtensionClientId,
            });

        _loggerMock = new Mock<ILogger<GraphService>>();

        _systemUnderTest = new GraphService(
            _graphServiceClientMock.Object,
            _azureB2CExtensionOptions.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task PatchUserProperty_DoesNotCallGraphServiceClient_WhenExtensionsClientIdIsMissing()
    {
        // Arrange
        _azureB2CExtensionOptions
            .Setup(x => x.Value)
            .Returns(new AzureB2CExtensionConfig
            {
                ExtensionsClientId = string.Empty
            });

        _systemUnderTest = new GraphService(
            _graphServiceClientMock.Object,
            _azureB2CExtensionOptions.Object,
            _loggerMock.Object);

        // Act
        await _systemUnderTest.PatchUserProperty(Guid.NewGuid(), "customProperty", "value");

        // Assert
        _requestAdapterMock
            .Verify(ra => ra.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task PatchUserProperty_CallsGraphServiceClientWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "custom";
        var value = "newValue";
        var propertyKey = $"extension_{ExtensionClientId.Replace("-", string.Empty)}_{propertyName}";

        // Act
        await _systemUnderTest.PatchUserProperty(userId, propertyName, value);

        // Assert
        _requestAdapterMock
            .Verify(ra => ra.SendAsync(
                It.Is<RequestInformation>(r => r.HttpMethod == Method.PATCH),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _requestAdapterMock
            .Verify(ra => ra.SendAsync(
                It.Is<RequestInformation>(info => ContentStreamContainsProperty(info.Content, propertyKey, value)),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task PatchUserProperty_LogsError_WhenSendAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "custom";
        var value = "newValue";
        var exception = new Exception("SendAsync failed");

        _requestAdapterMock
            .Setup(ra => ra.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _systemUnderTest.PatchUserProperty(userId, propertyName, value);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.VerifyLog(x => x.LogError("Error while trying to patch {PropertyName} for user {UserId} with Graph API", propertyName, userId));
    }

    [TestMethod]
    public async Task QueryUserProperty_DoesNotCallGraphServiceClient_WhenExtensionsClientIdIsMissing()
    {
        // Arrange
        _azureB2CExtensionOptions
            .Setup(x => x.Value)
            .Returns(new AzureB2CExtensionConfig
            {
                ExtensionsClientId = string.Empty
            });

        _systemUnderTest = new GraphService(
            _graphServiceClientMock.Object,
            _azureB2CExtensionOptions.Object,
            _loggerMock.Object);

        // Act
        await _systemUnderTest.QueryUserProperty(Guid.NewGuid(), "customProperty");

        // Assert
        _requestAdapterMock
            .Verify(ra => ra.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task QueryUserPropertyUserProperty_CallsGraphServiceClientWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "custom";

        // Act
        await _systemUnderTest.QueryUserProperty(userId, propertyName);

        // Assert
        _requestAdapterMock
            .Verify(ra => ra.SendAsync(
                It.Is<RequestInformation>(r => r.HttpMethod == Method.GET),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task QueryUserPropertyUserProperty_ReturnsExpectedResult_ForExistingProperty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "custom";
        var expected = "expected value";
        var propertyKey = $"extension_{ExtensionClientId.Replace("-", string.Empty)}_{propertyName}";

        _requestAdapterMock.Setup(adapter => adapter.SendAsync(
            It.Is<RequestInformation>(info => info.HttpMethod == Method.GET),
            User.CreateFromDiscriminatorValue,
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = $"{userId}",
                    AdditionalData = new Dictionary<string, object>
                    {
                        { propertyKey, expected }
                    }
                });

        // Act
        var result = await _systemUnderTest.QueryUserProperty(userId, propertyName);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public async Task QueryUserPropertyUserProperty_ReturnsNull_ForMissingProperty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "missing";

        _requestAdapterMock.Setup(adapter => adapter.SendAsync(
            It.Is<RequestInformation>(info => info.HttpMethod == Method.GET),
            User.CreateFromDiscriminatorValue,
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = $"{userId}",
                    AdditionalData = new Dictionary<string, object>()
                });

        // Act
        var result = await _systemUnderTest.QueryUserProperty(userId, propertyName);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task QueryUserProperty_LogsError_WhenGetAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var propertyName = "custom";
        var exception = new Exception("SendAsync failed");

        _requestAdapterMock
            .Setup(ra => ra.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<User>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _systemUnderTest.QueryUserProperty(userId, propertyName);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.VerifyLog(x => x.LogError("Error while trying to read {PropertyName} for user {UserId} with Graph API", propertyName, userId));
    }

    private static bool ContentStreamContainsProperty(Stream stream, string key, string? value)
    {
        if (stream == null || !stream.CanRead)
        {
            return false;
        }

        using StreamReader reader = new(stream);
        string contentString = reader.ReadToEnd();

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(contentString);

        return dict is not null && dict.ContainsKey(key) && dict[key] == value;
    }
}
