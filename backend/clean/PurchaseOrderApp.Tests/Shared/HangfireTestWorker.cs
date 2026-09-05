using System.Diagnostics;
using Hangfire;

namespace PurchaseOrderApp.Tests.Shared;

internal static class HangfireTestWorker
{
    public static async Task WaitForSuccessAsync(JobStorage storage, int expected, CancellationToken cancellationToken = default)
    {
        var timer = Stopwatch.StartNew();
        var monitor = storage.GetMonitoringApi();
        while (timer.Elapsed < TimeSpan.FromSeconds(30)) {
            if (monitor.SucceededListCount() >= expected) return;
            if (monitor.FailedCount() > 0) {
                var failure = monitor.FailedJobs(0, 1).First().Value;
                Assert.Fail($"Hangfire job failed: {failure.ExceptionMessage} {failure.ExceptionDetails}");
            }
            await Task.Delay(100, cancellationToken);
        }
        Assert.Fail($"Timed out waiting for {expected} successful Hangfire jobs. Scheduled: {monitor.ScheduledCount()}.");
    }
}
