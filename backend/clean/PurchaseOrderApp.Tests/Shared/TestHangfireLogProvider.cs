using Hangfire.Logging;

namespace PurchaseOrderApp.Tests.Shared;

// Hangfire logging is process-global; never retain a disposed WebApplicationFactory logger.
internal sealed class TestHangfireLogProvider : ILogProvider
{
    public ILog GetLogger(string name) => new Logger();
    private sealed class Logger : ILog
    {
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null!)
        {
            if (logLevel < LogLevel.Warn) return false;
            if (messageFunc is not null) TestContext.Progress.WriteLine($"{messageFunc()} {exception}");
            return true;
        }
    }
}