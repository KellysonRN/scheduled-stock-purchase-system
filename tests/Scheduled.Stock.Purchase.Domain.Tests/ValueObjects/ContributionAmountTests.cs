using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class ContributionAmountTests
{
    [Fact]
    public void Should_Create_Valid_ContributionAmount_From_Positive_Amount()
    {
        var result = ContributionAmount.Create(25.50m);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Value.Amount.ShouldBe(25.50m);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero()
    {
        var result = ContributionAmount.Create(0m);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(ContributionAmountErrors.ZeroContribution.Code);
        result.Error.Message.ShouldBe(ContributionAmountErrors.ZeroContribution.Message);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var result = ContributionAmount.Create(-5m);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(MoneyErrors.Negative.Code);
        result.Error.Message.ShouldBe(MoneyErrors.Negative.Message);
    }

    [Fact]
    public void Should_Consider_ContributionAmounts_With_Equal_Money_As_Equal()
    {
        var first = ContributionAmount.Create(100m).Value;
        var second = ContributionAmount.Create(100.00m).Value;

        first.ShouldBe(second);
        first!.GetHashCode().ShouldBe(second!.GetHashCode());
    }
}
