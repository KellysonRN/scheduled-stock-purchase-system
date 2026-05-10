using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public static class TradeIdErrors
{
    public static readonly Error Empty = new("TradeId.Empty", "TradeId cannot be empty.");
}
