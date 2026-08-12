using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApp.Api.Configuration;

/// <summary>
/// Configuration values used to connect the API to the application database.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name used by the options binder.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// PostgreSQL connection string used by the PurchaseOrderApp database context.
    /// </summary>
    [Required]
    public string PurchaseOrderDb { get; set; } = default!;
}
