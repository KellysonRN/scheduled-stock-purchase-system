using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.DeleteTrade;

internal sealed class DeleteTradeHandler(ITradeRepository tradeRepository)
    : IHandler<DeleteTradeRequest, Result<DeleteTradeResponse>>
{
    private readonly ITradeRepository _tradeRepository = tradeRepository;

    public async Task<Result<DeleteTradeResponse>> HandleAsync(
        DeleteTradeRequest request,
        CancellationToken cancellationToken
    )
    {
        var trade = await _tradeRepository.GetByIdAsync(
            TradeId.Create(request.Id).Value,
            cancellationToken
        );

        if (trade is null)
            return Result<DeleteTradeResponse>.Failure(TradeErrors.NotFound(request.Id));

        await _tradeRepository.DeleteAsync(TradeId.Create(request.Id).Value, cancellationToken);

        return Result<DeleteTradeResponse>.Success(new DeleteTradeResponse(request.Id));
    }
}
