using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public class TickerErrors
{
    public static readonly Error Required = new("Ticker.Required", "Ticker is required");

    public static readonly Error InvalidFormat = new(
        "Ticker.InvalidFormat",
        "Invalid ticker format"
    );
}
