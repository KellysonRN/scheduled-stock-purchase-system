using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class ContributionAmountTests
{
    [Fact]
    public void Should_Create_Valid_ContributionAmount_From_Positive_Amount()
    {
        var result = ContributionAmount.Create(25.50m);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(25.50m, result.Value!.Value.Amount);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        var result = ContributionAmount.Create(0m);

        Assert.True(result.IsFailure);
        Assert.Equal(ContributionAmountErrors.ZeroContribution.Code, result.Error.Code);
        Assert.Equal(ContributionAmountErrors.ZeroContribution.Message, result.Error.Message);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var result = ContributionAmount.Create(-5m);

        Assert.True(result.IsFailure);
        Assert.Equal(MoneyErrors.Negative.Code, result.Error.Code);
        Assert.Equal(MoneyErrors.Negative.Message, result.Error.Message);
    }

    [Fact]
    public void Should_Consider_ContributionAmounts_With_Equal_Money_As_Equal()
    {
        var first = ContributionAmount.Create(100m).Value;
        var second = ContributionAmount.Create(100.00m).Value;

        Assert.Equal(first, second);
        Assert.Equal(first!.GetHashCode(), second!.GetHashCode());
    }
}
