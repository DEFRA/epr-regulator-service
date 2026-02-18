using EPR.Common.Functions.CancellationTokens;
using EPR.Common.Functions.CancellationTokens.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]

namespace EPR.Common.Functions.Test.CancellationTokens;

[TestClass]
public class CancellationTokenAccessorTests
{
        [TestMethod]
        public void CancellationToken_WhenRequested_ReturnsCancellationToken()
        {
            // Arrange
            var cancellationTokenAccessor = GetCancellationTokenAccessor();

            // Act
            var cancellationToken = cancellationTokenAccessor.CancellationToken;

            // Assert
            cancellationToken.Should().NotBeNull();
        }

        [TestMethod]
        public void CancellationToken_WhenRequestedBeforeSet_ThrowsNotSupportedException()
        {
            // Arrange
            var cancellationTokenAccessor = new CancellationTokenAccessor(Substitute.For<IHttpContextAccessor>(), Substitute.For<ILogger<CancellationTokenAccessor>>());

            // Act
            var act = () => cancellationTokenAccessor.CancellationToken;

            // Assert
            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void CancellationToken_WhenSuppliedTokenCancelled_ReturnsCancelledToken()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationTokenAccessor = GetCancellationTokenAccessor(cancellationTokenSource.Token);

            // Act
            cancellationTokenSource.Cancel();
            var cancellationToken = cancellationTokenAccessor.CancellationToken;

            // Assert
            cancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [TestMethod]
        public void CancellationToken_WhenRequestAbortedTokenCancelled_ReturnsCancelledToken()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationTokenAccessor = GetCancellationTokenAccessor(httpRequestAbortedCancellationToken: cancellationTokenSource.Token);

            // Act
            cancellationTokenSource.Cancel();
            var cancellationToken = cancellationTokenAccessor.CancellationToken;

            // Assert
            cancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [TestMethod]
        public void CancellationToken_WhenSuppliedTokenCancelled_CancelsExistingToken()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationTokenAccessor = GetCancellationTokenAccessor(cancellationTokenSource.Token);

            // Act
            var cancellationToken = cancellationTokenAccessor.CancellationToken;
            cancellationTokenSource.Cancel();

            // Assert
            cancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [TestMethod]
        public void CancellationToken_WhenRequestAbortedTokenCancelled_CancelsExistingToken()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationTokenAccessor = GetCancellationTokenAccessor(httpRequestAbortedCancellationToken: cancellationTokenSource.Token);

            // Act
            var cancellationToken = cancellationTokenAccessor.CancellationToken;
            cancellationTokenSource.Cancel();

            // Assert
            cancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [TestMethod]
        public void CancellationToken_WhenContainerDisposedWithoutInitialization_Succeeds()
        {
            // Arrange
            var cancellationTokenAccessor = new CancellationTokenAccessor(Substitute.For<IHttpContextAccessor>(), Substitute.For<ILogger<CancellationTokenAccessor>>());

            // Act
            var act = () => cancellationTokenAccessor.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Dispose_WhenInitialized_DisposesTokenSource()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.RequestAborted = CancellationToken.None;
            var cancellationTokenAccessor = new CancellationTokenAccessor(httpContextAccessor, Substitute.For<ILogger<CancellationTokenAccessor>>());
            cancellationTokenAccessor.CancellationToken = CancellationToken.None;

            // Act
            var act = () => cancellationTokenAccessor.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void CancellationToken_WhenHttpContextAccessorIsNull_StillWorks()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns((HttpContext)null);
            var cancellationTokenAccessor = new CancellationTokenAccessor(httpContextAccessor, Substitute.For<ILogger<CancellationTokenAccessor>>());

            // Act
            cancellationTokenAccessor.CancellationToken = CancellationToken.None;
            var token = cancellationTokenAccessor.CancellationToken;

            // Assert
            token.Should().NotBeNull();
        }

        private static ICancellationTokenAccessor GetCancellationTokenAccessor(CancellationToken? suppliedCancellationToken = null, CancellationToken? httpRequestAbortedCancellationToken = null)
        {
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var cancellationTokenSource = new CancellationTokenSource();
            httpContextAccessor.HttpContext.RequestAborted = httpRequestAbortedCancellationToken ?? CancellationToken.None;
            var cancellationTokenAccessor = new CancellationTokenAccessor(httpContextAccessor, Substitute.For<ILogger<CancellationTokenAccessor>>());
            cancellationTokenAccessor.CancellationToken = suppliedCancellationToken ?? CancellationToken.None;
            return cancellationTokenAccessor;
        }
}