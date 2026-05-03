using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed class Quantity : IEquatable<Quantity>
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Result<Quantity> Create(int value)
    {
        return value <= 0
            ? Result<Quantity>.Failure(QuantityErrors.Negative)
            : Result<Quantity>.Success(new Quantity(value));
    }

    public bool Equals(Quantity? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}
