using PurchaseOrderApp.Domain.Services;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain;

[TestFixture]
public sealed class PurchaseOrderNumberGeneratorTests
{
    [Test]
    public void Create_ShouldReturnPurchaseOrderNumberWithFiveDigits()
    {
        // Act
        var purchaseOrderNumber = PurchaseOrderNumberGenerator.Create();

        // Assert
        purchaseOrderNumber.ShouldStartWith("PO-");
        purchaseOrderNumber.Length.ShouldBe(8);
        purchaseOrderNumber[3..].All(char.IsDigit).ShouldBeTrue();
    }
}
