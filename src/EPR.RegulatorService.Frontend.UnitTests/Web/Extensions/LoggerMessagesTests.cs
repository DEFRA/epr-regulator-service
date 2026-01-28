using EPR.RegulatorService.Frontend.Web.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Extensions
{
    [TestClass]
    public class LoggerMessagesTests
    {
        [TestMethod]
        public void RedisConnectionException_LogsErrorWithMessage()
        {
            string message = "connection timed out";
            var testLogger = new TestLogger();
            testLogger.Logger.RedisConnectionException(message);

            Assert.AreEqual(1, testLogger.Entries.Count);
            var entry = testLogger.Entries.Single();
            Assert.AreEqual(LogLevel.Error, entry.Level);
            Assert.AreEqual("L2 Cache Failure Redis connection exception: connection timed out", entry.Message);
            Assert.IsTrue(entry.State.TryGetValue("Message", out object? stateValue));
            Assert.AreEqual(message, stateValue);
        }

        [TestMethod]
        public void RedisCacheFailure_LogsErrorWithMessage()
        {
            string message = "cache write failed";
            var testLogger = new TestLogger();
            testLogger.Logger.RedisCacheFailure(message);

            Assert.AreEqual(1, testLogger.Entries.Count);
            var entry = testLogger.Entries.Single();
            Assert.AreEqual(LogLevel.Error, entry.Level);
            Assert.AreEqual("L2 Cache Failure: cache write failed", entry.Message);
            Assert.IsTrue(entry.State.TryGetValue("Message", out object? stateValue));
            Assert.AreEqual(message, stateValue);
        }

        private sealed class TestLogger : ILogger
        {
            public IList<LogEntry> Entries { get; } = new List<LogEntry>();

            public ILogger Logger => this;

            ILogger BeginScope<TState>(TState state) => NullLogger.Instance;
            bool IsEnabled(LogLevel logLevel) => true;

            void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                string formatted = formatter(state, exception);
                var stateDictionary = new Dictionary<string, object?>();
                if (state is IEnumerable<KeyValuePair<string, object?>> kvps)
                {
                    foreach (KeyValuePair<string, object?> kvp in kvps)
                    {
                        // Some logging states include the original format string under "{OriginalFormat}" or "OriginalFormat".
                        // We only capture the named properties.
                        if (!stateDictionary.ContainsKey(kvp.Key))
                        {
                            stateDictionary[kvp.Key] = kvp.Value;
                        }
                    }
                }

                Entries.Add(new LogEntry
                {
                    Level = logLevel,
                    EventId = eventId,
                    Message = formatted,
                    Exception = exception,
                    State = stateDictionary
                });
            }

            // Explicit interface implementation so this class can be used as ILogger directly
            IDisposable ILogger.BeginScope<TState>(TState state) => (IDisposable)BeginScope(state);
            bool ILogger.IsEnabled(LogLevel logLevel) => IsEnabled(logLevel);
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => Log(logLevel, eventId, state, exception, formatter);
        }

        private sealed class LogEntry
        {
            public LogLevel Level { get; init; }
            public EventId EventId { get; init; }
            public string Message { get; init; } = string.Empty;
            public Exception? Exception { get; init; }
            public IReadOnlyDictionary<string, object?> State { get; init; } = new Dictionary<string, object?>();
        }

        // Minimal null logger used for BeginScope implementation.
        private sealed class NullLogger : IDisposable, ILogger
        {
            public static NullLogger Instance { get; } = new NullLogger();
            public void Dispose() { }

            IDisposable ILogger.BeginScope<TState>(TState state) => Instance;
            bool ILogger.IsEnabled(LogLevel logLevel) => false;
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}