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
        // Arrange
        const decimal value = 12.50m;

        // Act
        var money = new Money(value);

        // Assert
        money.Value.ShouldBe(12.50m);
    }

    [Test]
    public void Constructor_ShouldThrow_WhenMoneyIsNegative()
    {
        // Arrange
        const decimal value = -0.01m;

        // Act
        var exception = Should.Throw<DomainException>(() => new Money(value));

        // Assert
        exception.Message.ShouldBe("Money cannot be negative.");
    }
}
