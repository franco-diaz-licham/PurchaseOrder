using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApp.Api.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string PurchaseOrderDb { get; set; } = default!;
}
