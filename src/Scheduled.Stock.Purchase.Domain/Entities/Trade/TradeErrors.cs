using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.Entities;

public static class TradeErrors
{
    public static readonly Error InvalidTicker = new("Trade.InvalidTicker", "Invalid ticker");

    public static readonly Error InvalidQuantity = new("Trade.InvalidQuantity", "Invalid quantity");

    public static readonly Error InvalidPrice = new("Trade.InvalidPrice", "Invalid price");
}
