using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed record TradeId
{
    public Guid Value { get; }

    private TradeId(Guid value)
    {
        Value = value;
    }

    public static Result<TradeId> Create(Guid value) =>
        value == Guid.Empty
            ? Result<TradeId>.Failure(TradeIdErrors.Empty)
            : Result<TradeId>.Success(new TradeId(value));

    public static TradeId New() => Create(Guid.NewGuid()).Value!;

    public override string ToString() => Value.ToString();
}
