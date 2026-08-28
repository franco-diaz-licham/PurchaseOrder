namespace PurchaseOrderApp.Domain.Core;

/// <summary>
/// Represents an expected business rule failure inside the domain.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Creates a domain exception with the business rule failure message.
    /// </summary>
    public DomainException(string message) : base(message) { }
}
