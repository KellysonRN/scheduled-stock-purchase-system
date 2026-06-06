namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal sealed class CreateTradeRequest
{
    public string Ticker { get; init; } = default!;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string Type { get; init; } = default!;
}
