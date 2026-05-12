using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.Entities;

public sealed class Trade : Entity<TradeId>
{
    public Ticker Ticker { get; private set; } = default!;

    public decimal Quantity { get; private set; }

    public Money Price { get; private set; } = default!;

    public TradeType Type { get; private set; }

    public DateTime ExecutedAt { get; private set; }

    private Trade() { }

    private Trade(
        TradeId id,
        Ticker ticker,
        decimal quantity,
        Money price,
        TradeType type,
        DateTime executedAt
    )
    {
        Id = id;
        Ticker = ticker;
        Quantity = quantity;
        Price = price;
        Type = type;
        ExecutedAt = executedAt;
    }

    public static Result<Trade> Buy(Ticker ticker, decimal quantity, Money price)
    {
        return Create(ticker, quantity, price, TradeType.Buy);
    }

    public static Result<Trade> Sell(Ticker ticker, decimal quantity, Money price)
    {
        return Create(ticker, quantity, price, TradeType.Sell);
    }

    private static Result<Trade> Create(
        Ticker ticker,
        decimal quantity,
        Money price,
        TradeType type
    )
    {
        if (ticker is null)
            return Result<Trade>.Failure(TradeErrors.InvalidTicker);

        if (quantity <= 0)
            return Result<Trade>.Failure(TradeErrors.InvalidQuantity);

        if (price is null || price.Amount <= 0)
            return Result<Trade>.Failure(TradeErrors.InvalidPrice);

        var trade = new Trade(TradeId.New(), ticker, quantity, price, type, DateTime.UtcNow);

        return Result<Trade>.Success(trade);
    }
}
