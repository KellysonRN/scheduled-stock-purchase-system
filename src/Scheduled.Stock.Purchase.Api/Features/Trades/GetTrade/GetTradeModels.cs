namespace Scheduled.Stock.Purchase.Api.Features.Trades.GetTrade;

public sealed record GetTradeRequest(Guid Id);

public sealed record GetTradeResponse(
    Guid Id,
    string Ticker,
    decimal Quantity,
    decimal Price,
    string Type,
    DateTime ExecutedAt
);
