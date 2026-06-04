using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.DeleteTrade;

internal sealed class DeleteTradeHandler(ITradeRepository tradeRepository)
    : IHandler<DeleteTradeRequest, Result<Unit>>
{
    private readonly ITradeRepository _tradeRepository = tradeRepository;

    public async Task<Result<Unit>> HandleAsync(
        DeleteTradeRequest request,
        CancellationToken cancellationToken
    )
    {
        var trade = await _tradeRepository.GetByIdAsync(new TradeId(request.Id), cancellationToken);

        if (trade is null)
            return Result<Unit>.Failure(TradeErrors.NotFound(request.Id));

        await _tradeRepository.DeleteAsync(new TradeId(request.Id), cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
