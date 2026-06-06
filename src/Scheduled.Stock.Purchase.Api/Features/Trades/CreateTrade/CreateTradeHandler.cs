using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal sealed class CreateTradeHandler(ITradeRepository tradeRepository)
    : IHandler<CreateTradeRequest, Result<CreateTradeResponse>>
{
    private readonly ITradeRepository _tradeRepository = tradeRepository;

    public async Task<Result<CreateTradeResponse>> HandleAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<TradeType>(request.Type, true, out var tradeType))
        {
            return Result<CreateTradeResponse>.Failure(CreateTradeErrors.InvalidType);
        }

        var tickerResult = Ticker.Create(request.Ticker);
        if (tickerResult.IsFailure)
            return Result<CreateTradeResponse>.Failure(tickerResult.Error);

        var priceResult = Money.Create(request.Price);
        if (priceResult.IsFailure)
            return Result<CreateTradeResponse>.Failure(priceResult.Error);

        var quantityResult = Quantity.Create(request.Quantity);
        if (quantityResult.IsFailure)
            return Result<CreateTradeResponse>.Failure(quantityResult.Error);

        var tradeResult =
            tradeType == TradeType.Buy
                ? Trade.Buy(tickerResult.Value!, quantityResult.Value!, priceResult.Value!)
                : Trade.Sell(tickerResult.Value!, quantityResult.Value!, priceResult.Value!);

        if (tradeResult.IsFailure)
            return Result<CreateTradeResponse>.Failure(tradeResult.Error);

        var trade = tradeResult.Value!;

        await _tradeRepository.AddAsync(trade, cancellationToken);

        var response = new CreateTradeResponse(
            trade.Id.Value,
            trade.Ticker.Value,
            trade.Quantity.Value,
            trade.Price.Amount,
            trade.Type,
            trade.ExecutedAt
        );

        return Result<CreateTradeResponse>.Success(response);
    }
}
