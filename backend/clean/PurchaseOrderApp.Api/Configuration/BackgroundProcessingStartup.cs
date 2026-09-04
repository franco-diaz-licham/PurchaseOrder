using Hangfire;
using Microsoft.Extensions.Options;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Api.Configuration;

public static class BackgroundProcessingStartup
{
    public static WebApplication UseBackgroundProcessingDashboard(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<BackgroundProcessingOptions>>().Value;
        if (app.Environment.IsDevelopment() && options.Enabled) app.UseHangfireDashboard("/jobs");

        return app;
    }

    public static WebApplication ScheduleBackgroundProcessing(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<BackgroundProcessingOptions>>().Value;
        if (!options.Enabled) return app;

        RecurringJob.AddOrUpdate<IOutboxProcessor>(
            "background-outbox-processor",
            processor => processor.ProcessPendingAsync(CancellationToken.None),
            options.OutboxProcessingCron);

        return app;
    }
}
