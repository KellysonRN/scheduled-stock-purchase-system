using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal static class CreateTradeErrors
{
    public static readonly Error InvalidType = new(
        "Trade.InvalidType",
        "Trade type must be Buy or Sell."
    );
}
