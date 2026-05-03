using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public static class MoneyErrors
{
    public static readonly Error Negative = new("Money.Negative", "Money cannot be negative");

    public static readonly Error InvalidLength = new("Money.InvalidLength", "Insufficient amount");
}
