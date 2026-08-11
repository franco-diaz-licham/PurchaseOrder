namespace PurchaseOrderApp.Domain.Core;

/// <summary>
/// Represents an expected business rule failure inside the domain.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
