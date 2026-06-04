namespace Scheduled.Stock.Purchase.Api.Features.Trades.UpdateTrade;

public sealed record UpdateTradeRequest(
    Guid Id,
    string Ticker,
    int Quantity,
    decimal Price,
    string Type
);

public sealed record UpdateTradeResponse(
    Guid Id,
    string Ticker,
    int Quantity,
    decimal Price,
    string Type,
    DateTime ExecutedAt
);
