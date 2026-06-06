using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.GetTrade;

internal sealed class GetTradeHandler(ITradeRepository tradeRepository)
    : IHandler<GetTradeRequest, Result<GetTradeResponse>>
{
    private readonly ITradeRepository _tradeRepository = tradeRepository;

    public async Task<Result<GetTradeResponse>> HandleAsync(
        GetTradeRequest request,
        CancellationToken cancellationToken
    )
    {
        var trade = await _tradeRepository.GetByIdAsync(
            TradeId.Create(request.Id).Value,
            cancellationToken
        );

        if (trade is null)
            return Result<GetTradeResponse>.Failure(TradeErrors.NotFound(request.Id));

        var response = new GetTradeResponse(
            trade.Id.Value,
            trade.Ticker.Value,
            trade.Quantity.Value,
            trade.Price.Amount,
            trade.Type.ToString(),
            trade.ExecutedAt
        );

        return Result<GetTradeResponse>.Success(response);
    }
}
