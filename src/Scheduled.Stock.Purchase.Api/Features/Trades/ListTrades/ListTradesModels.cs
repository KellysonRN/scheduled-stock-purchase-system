namespace Scheduled.Stock.Purchase.Api.Features.Trades.ListTrades;

public sealed record ListTradesRequest(int PageNumber, int PageSize);

public sealed record TradeItem(
    Guid Id,
    string Ticker,
    decimal Quantity,
    decimal Price,
    string Type,
    DateTime ExecutedAt
);

public sealed record ListTradesResponse(
    List<TradeItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount
);
