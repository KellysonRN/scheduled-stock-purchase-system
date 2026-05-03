using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed class ContributionAmount : IEquatable<ContributionAmount>
{
    public Money Value { get; }

    private ContributionAmount(Money value)
    {
        Value = value;
    }

    public static Result<ContributionAmount> Create(decimal amount)
    {
        var moneyResult = Money.Create(amount);

        if (moneyResult.IsFailure)
            return Result<ContributionAmount>.Failure(moneyResult.Error);

        var money = moneyResult.Value;

        return money is null || money.Amount == 0
            ? Result<ContributionAmount>.Failure(ContributionAmountErrors.ZeroContribution)
            : Result<ContributionAmount>.Success(new ContributionAmount(money));
    }

    public bool Equals(ContributionAmount? other) => other is not null && Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();
}
