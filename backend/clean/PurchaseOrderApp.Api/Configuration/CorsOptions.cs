namespace PurchaseOrderApp.Api.Configuration;

/// <summary>
/// Configuration values used to control browser origins allowed to call the API.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// Configuration section name used by the options binder.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Named CORS policy registered with ASP.NET Core.
    /// </summary>
    public const string PolicyName = "ConfiguredCors";

    /// <summary>
    /// Browser origins allowed to call the API.
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];
}
