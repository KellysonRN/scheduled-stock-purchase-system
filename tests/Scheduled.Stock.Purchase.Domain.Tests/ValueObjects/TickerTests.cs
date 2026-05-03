using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class TickerTests
{
    [Theory]
    [InlineData("AAPL1", "AAPL1")]
    [InlineData("goog12", "GOOG12")]
    [InlineData(" msft2 ", "MSFT2")]
    public void Should_Create_Valid_Ticker(string value, string expected)
    {
        var result = Ticker.Create(value);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expected, result.Value!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Ticker_Is_Missing(string? value)
    {
        var result = Ticker.Create(value!);

        Assert.True(result.IsFailure);
        Assert.Equal(TickerErrors.Required.Code, result.Error.Code);
        Assert.Equal(TickerErrors.Required.Message, result.Error.Message);
    }

    [Theory]
    [InlineData("ABC1")]          // too short
    [InlineData("ABCDE1")]        // too many letters
    [InlineData("ABCD")]          // missing digits
    [InlineData("ABCD123")]       // too many digits
    [InlineData("AB1D2")]         // invalid letter/digit order
    [InlineData("ABCD!1")]        // invalid character
    public void Should_Fail_When_Ticker_Has_Invalid_Format(string value)
    {
        var result = Ticker.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(TickerErrors.InvalidFormat.Code, result.Error.Code);
        Assert.Equal(TickerErrors.InvalidFormat.Message, result.Error.Message);
    }

    [Fact]
    public void Should_Consider_Tickers_With_Same_Normalized_Value_As_Equal()
    {
        var ticker1 = Ticker.Create("goog12").Value;
        var ticker2 = Ticker.Create(" GOOG12 ").Value;

        Assert.Equal(ticker1, ticker2);
        Assert.Equal(ticker1!.GetHashCode(), ticker2!.GetHashCode());
    }
}
