using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApp.Api.Configuration;

public sealed class BackgroundProcessingOptions
{
    public const string SectionName = "BackgroundProcessing";

    /// <summary>Controls the worker, recurring relay, and development dashboard.</summary>
    public bool Enabled { get; set; } = true;

    [Required]
    public string OutboxProcessingCron { get; set; } = "* * * * *";

    [Range(1, 20)]
    public int WorkerCount { get; set; } = 1;
}
