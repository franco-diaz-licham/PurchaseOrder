namespace PurchaseOrderApp.Services.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public const string PolicyName = "ConfiguredCors";

    public string[] AllowedOrigins { get; init; } = [];
}
