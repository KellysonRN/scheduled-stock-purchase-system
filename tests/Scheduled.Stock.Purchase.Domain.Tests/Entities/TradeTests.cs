using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

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

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsSuccess.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();
        price.ShouldNotBeNull();

        //
        var result = Trade.Buy(ticker, quantity, price);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var trade = result.Value;

        trade.ShouldNotBeNull();
        trade.Ticker.ShouldBe(ticker);
        trade.Quantity.ShouldBe(quantity);
        trade.Price.ShouldBe(price);
        trade.Type.ShouldBe(TradeType.Buy);
        (DateTime.UtcNow - trade.ExecutedAt).ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Should_Create_Sell_Trade_When_Data_Is_Valid()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(30m);
        var quantity = 5m;

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsSuccess.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();
        price.ShouldNotBeNull();

        // Act
        var result = Trade.Sell(ticker, quantity, price);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var trade = result.Value;
        trade.ShouldNotBeNull();
        trade.Type.ShouldBe(TradeType.Sell);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Fail_When_Quantity_Is_Invalid(decimal quantity)
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsSuccess.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();
        price.ShouldNotBeNull();

        // Act
        var result = Trade.Buy(ticker, quantity, price);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TradeErrors.InvalidQuantity);
    }

    [Fact]
    public void Should_Fail_When_Ticker_Is_Null()
    {
        // Arrange
        Ticker ticker = null!;
        var priceResult = Money.Create(10m);

        priceResult.IsSuccess.ShouldBeTrue();

        var price = priceResult.Value;

        price.ShouldNotBeNull();

        // Act
        var result = Trade.Buy(ticker!, 10m, price);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TradeErrors.InvalidTicker);
    }

    [Theory]
    [InlineData(-5)]
    public void Should_Fail_When_Price_Is_Invalid(decimal priceValue)
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(priceValue);

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsFailure.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();

        // Act
        var result = Trade.Buy(ticker, 10m, price!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TradeErrors.InvalidPrice);
    }

    [Fact]
    public void Should_Set_ExecutedAt_When_Trade_Is_Created()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsSuccess.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();
        price.ShouldNotBeNull();

        // Act
        var result = Trade.Buy(ticker, 10m, price);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var trade = result.Value;
        trade.ShouldNotBeNull();
        trade.ExecutedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Should_Generate_Different_Ids_For_Different_Trades()
    {
        // Arrange
        var tickerResult = Ticker.Create("ITUB4");
        var priceResult = Money.Create(10m);

        tickerResult.IsSuccess.ShouldBeTrue();
        priceResult.IsSuccess.ShouldBeTrue();

        var ticker = tickerResult.Value;
        var price = priceResult.Value;

        ticker.ShouldNotBeNull();
        price.ShouldNotBeNull();

        // Act
        var trade1 = Trade.Buy(ticker, 10m, price).Value;
        var trade2 = Trade.Buy(ticker, 10m, price).Value;

        // Assert
        trade1.ShouldNotBeNull();
        trade2.ShouldNotBeNull();
        trade1.Id.ShouldNotBe(trade2.Id);
    }
}
