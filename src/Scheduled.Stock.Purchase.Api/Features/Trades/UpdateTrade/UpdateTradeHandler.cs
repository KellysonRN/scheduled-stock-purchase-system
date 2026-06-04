using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.UpdateTrade;

internal sealed class UpdateTradeHandler : IHandler<UpdateTradeRequest, Result<UpdateTradeResponse>>
{
    private readonly ITradeRepository _tradeRepository;

    public UpdateTradeHandler(ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<Result<UpdateTradeResponse>> HandleAsync(
        UpdateTradeRequest request,
        CancellationToken cancellationToken
    )
    {
        var trade = await _tradeRepository.GetByIdAsync(new TradeId(request.Id), cancellationToken);

        if (trade is null)
            return Result<UpdateTradeResponse>.Failure(TradeErrors.NotFound(request.Id));

        var tickerResult = Ticker.Create(request.Ticker);
        if (tickerResult.IsFailure)
            return Result<UpdateTradeResponse>.Failure(tickerResult.Error);

        var quantityResult = Quantity.Create(request.Quantity);
        if (quantityResult.IsFailure)
            return Result<UpdateTradeResponse>.Failure(quantityResult.Error);

        var priceResult = Money.Create(request.Price);
        if (priceResult.IsFailure)
            return Result<UpdateTradeResponse>.Failure(priceResult.Error);

        var updatedTrade =
            request.Type.ToUpper() == "BUY"
                ? Trade.Buy(tickerResult.Value, quantityResult.Value, priceResult.Value)
                : Trade.Sell(tickerResult.Value, quantityResult.Value, priceResult.Value);

        await _tradeRepository.UpdateAsync(updatedTrade, cancellationToken);

        var response = new UpdateTradeResponse(
            updatedTrade.Id.Value,
            updatedTrade.Ticker.Value,
            updatedTrade.Quantity.Value,
            updatedTrade.Price.Amount,
            updatedTrade.Type.ToString(),
            updatedTrade.ExecutedAt
        );

        return Result<UpdateTradeResponse>.Success(response);
    }
}
