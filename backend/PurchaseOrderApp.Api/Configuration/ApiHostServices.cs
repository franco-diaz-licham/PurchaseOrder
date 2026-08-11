using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseOrderApp.Api.Configuration;

public static class ApiHostServices
{
    public static WebApplicationBuilder AddHostServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.ConfigureHttpJsonOptions(options => {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        });

        return builder;
    }
}
