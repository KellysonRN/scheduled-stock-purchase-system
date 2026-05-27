using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal sealed class CreateTradeHandler : IHandler<CreateTradeRequest, Result<CreateTradeResponse>>
{
    public Task<Result<CreateTradeResponse>> HandleAsync(CreateTradeRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TradeType>(request.Type, true, out var tradeType))
        {
            return Task.FromResult(Result<CreateTradeResponse>.Failure(CreateTradeErrors.InvalidType));
        }

        var tickerResult = Ticker.Create(request.Ticker);
        if (tickerResult.IsFailure)
            return Task.FromResult(Result<CreateTradeResponse>.Failure(tickerResult.Error));

        var priceResult = Money.Create(request.Price);
        if (priceResult.IsFailure)
            return Task.FromResult(Result<CreateTradeResponse>.Failure(priceResult.Error));

        var tradeResult = tradeType == TradeType.Buy
            ? Trade.Buy(tickerResult.Value!, request.Quantity, priceResult.Value!)
            : Trade.Sell(tickerResult.Value!, request.Quantity, priceResult.Value!);

        if (tradeResult.IsFailure)
            return Task.FromResult(Result<CreateTradeResponse>.Failure(tradeResult.Error));

        var trade = tradeResult.Value!;
        var response = new CreateTradeResponse(
            trade.Id.Value,
            trade.Ticker.Value,
            trade.Quantity,
            trade.Price.Amount,
            trade.Type,
            trade.ExecutedAt
        );

        return Task.FromResult(Result<CreateTradeResponse>.Success(response));
    }
}
