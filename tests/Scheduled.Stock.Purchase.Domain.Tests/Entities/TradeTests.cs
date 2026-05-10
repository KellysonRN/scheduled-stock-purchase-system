using FluentAssertions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Domain.Tests.Entities;

public class TradeTests
{
    [Fact]
    public void Should_Create_Buy_Trade_When_Data_Is_Valid()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(25m);
        var quantity = 10m;

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsSuccess.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();
        price.Should().NotBeNull();

        // Act
        var result = Trade.Buy(ticker, quantity, price);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var trade = result.Value;

        trade.Should().NotBeNull();
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
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(30m);
        var quantity = 5m;

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsSuccess.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();
        price.Should().NotBeNull();

        // Act
        var result = Trade.Sell(ticker, quantity, price);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var trade = result.Value;
        trade.Should().NotBeNull();
        trade.Type.Should().Be(TradeType.Sell);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Fail_When_Quantity_Is_Invalid(decimal quantity)
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsSuccess.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();
        price.Should().NotBeNull();

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
        var priceResult = Money.Create(10m);

        priceResult.IsSuccess.Should().BeTrue();

        var price = priceResult.Value;

        price.Should().NotBeNull();

        // Act
        var result = Trade.Buy(ticker!, 10m, price);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TradeErrors.InvalidTicker);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Fail_When_Price_Is_Invalid(decimal priceValue)
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(priceValue);

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsFailure.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();

        // Act
        var result = Trade.Buy(ticker, 10m, price!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TradeErrors.InvalidPrice);
    }

    [Fact]
    public void Should_Set_ExecutedAt_When_Trade_Is_Created()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsSuccess.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();
        price.Should().NotBeNull();

        // Act
        var result = Trade.Buy(ticker, 10m, price);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var trade = result.Value;
        trade.Should().NotBeNull();
        trade.ExecutedAt.Should().NotBe(default);
    }

    [Fact]
    public void Should_Generate_Different_Ids_For_Different_Trades()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.Should().BeTrue();
        priceResult.IsSuccess.Should().BeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.Should().NotBeNull();
        price.Should().NotBeNull();

        // Act
        var trade1 = Trade.Buy(ticker, 10m, price).Value;
        var trade2 = Trade.Buy(ticker, 10m, price).Value;

        // Assert
        trade1.Should().NotBeNull();
        trade2.Should().NotBeNull();
        trade1.Id.Should().NotBe(trade2.Id);
    }
}
