using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Domain.Tests.Entities;

public class TradeTests
{
    [Fact]
    public void Should_Create_Buy_Trade_When_Data_Is_Valid()
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");
        var quantity = 10m;
        var price = Money.Create(25m);

        // Act
        var result = Trade.Buy(ticker, quantity, price);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var trade = result.Value;

        trade.Ticker.Should().Be(ticker);
        trade.Quantity.Should().Be(quantity);
        trade.Price.Should().Be(price);
        trade.Type.Should().Be(TradeType.Buy);
        trade.ExecutedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Should_Create_Sell_Trade_When_Data_Is_Valid()
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");

        // Act
        var result = Trade.Sell(ticker, 5, Money.Create(30));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(TradeType.Sell);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Fail_When_Quantity_Is_Invalid(decimal quantity)
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");
        var price = Money.Create(10);

        // Act
        var result = Trade.Buy(ticker, quantity, price);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TradeErrors.InvalidQuantity);
    }

    [Fact]
    public void Should_Fail_When_Ticker_Is_Null()
    {
        // Arrange
        Ticker ticker = null!;
        var price = Money.Create(10);

        // Act
        var result = Trade.Buy(ticker, 10, price);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TradeErrors.InvalidTicker);
    }

    [Fact]
    public void Should_Fail_When_Price_Is_Invalid()
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");

        // Act
        var result = Trade.Buy(ticker, 10, Money.Create(0));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TradeErrors.InvalidPrice);
    }

    [Fact]
    public void Should_Set_ExecutedAt_When_Trade_Is_Created()
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");

        // Act
        var result = Trade.Buy(ticker, 10, Money.Create(10));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExecutedAt.Should().NotBe(default);
    }

    [Fact]
    public void Should_Generate_Different_Ids_For_Different_Trades()
    {
        // Arrange
        var ticker = Ticker.Create("ITUB4");

        // Act
        var trade1 = Trade.Buy(ticker, 10, Money.Create(10)).Value;
        var trade2 = Trade.Buy(ticker, 10, Money.Create(10)).Value;
        // Assert
        trade1.Id.Should().NotBe(trade2.Id);
    }
}