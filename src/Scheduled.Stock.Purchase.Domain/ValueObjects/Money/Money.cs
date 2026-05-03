using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed partial class Money : IEquatable<Money>
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Result<Money> Create(decimal amount) =>
        amount < 0
            ? Result<Money>.Failure(MoneyErrors.Negative)
            : Result<Money>.Success(new Money(decimal.Round(amount, 2)));

    public Result<Money> Add(Money other) =>
        Result<Money>.Success(new Money(Amount + other.Amount));

    public Result<Money> Subtract(Money other)
    {
        return Amount < other.Amount
            ? Result<Money>.Failure(MoneyErrors.InvalidLength)
            : Result<Money>.Success(new Money(Amount - other.Amount));
    }

    public Result<Money> Multiply(int factor) => Result<Money>.Success(new Money(Amount * factor));

    public bool Equals(Money? other) => other is not null && Amount == other.Amount;

    public override int GetHashCode() => Amount.GetHashCode();
}
