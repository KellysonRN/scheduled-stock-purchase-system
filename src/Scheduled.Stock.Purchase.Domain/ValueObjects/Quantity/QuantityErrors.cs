using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public class QuantityErrors
{
    public static readonly Error Negative = new(
        "Quantity.Negative",
        "Quantity must be greater than zero"
    );
}
