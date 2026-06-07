using Scheduled.Stock.Purchase.Domain.Entities;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal sealed class CreateTradeResponse
{
    public Guid Id { get; init; }
    public string Ticker { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public TradeType Type { get; init; }
    public DateTime ExecutedAt { get; init; }

    public CreateTradeResponse(
        Guid id,
        string ticker,
        int quantity,
        decimal price,
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
}
