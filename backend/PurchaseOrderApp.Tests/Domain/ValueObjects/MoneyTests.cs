using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.ValueObjects;

[TestFixture]
public sealed class MoneyTests
{
    [Test]
    public void Constructor_ShouldSetValue()
    {
        // Act
        var money = new Money(12.50m);

        // Assert
        money.Value.ShouldBe(12.50m);
    }

    [Test]
    public void Constructor_ShouldThrow_WhenMoneyIsNegative()
    {
        // Act
        var exception = Should.Throw<DomainException>(() => new Money(-0.01m));

        // Assert
        exception.Message.ShouldBe("Money cannot be negative.");
    }
}
