namespace PurchaseOrderApp.Api.Configuration;

public static class ApiHostServices
{
    public static WebApplicationBuilder AddHostServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }
}
