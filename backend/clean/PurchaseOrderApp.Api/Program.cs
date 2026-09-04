using PurchaseOrderApp.Api.Configuration;

try {
    var builder = WebApplication.CreateBuilder(args);
    builder.AddHostServices();
    builder.Services.AddAppServices(builder.Configuration);
    var app = builder.Build();

    await app.MigrateDatabaseAsync();

    if (app.Environment.IsDevelopment()) {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseApiExceptionHandling();
    app.UseHttpsRedirection();
    app.UseCors(CorsOptions.PolicyName);
    app.MapControllers();
    app.UseBackgroundProcessingDashboard();
    app.ScheduleBackgroundProcessing();

    await app.RunAsync();
} catch {
    throw;
}

public partial class Program;
