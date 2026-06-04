using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Infrastructure.Repositories;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.ListTrades;

internal sealed class ListTradesHandler : IHandler<ListTradesRequest, Result<ListTradesResponse>>
{
    private readonly ITradeRepository _tradeRepository;

    public ListTradesHandler(ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<Result<ListTradesResponse>> HandleAsync(
        ListTradesRequest request,
        CancellationToken cancellationToken
    )
    {
        var trades = await _tradeRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var totalCount = await _tradeRepository.GetTotalCountAsync(cancellationToken);

        var items = trades
            .Select(t => new TradeItem(
                t.Id.Value,
                t.Ticker.Value,
                t.Quantity,
                t.Price.Amount,
                t.Type.ToString(),
                t.ExecutedAt
            ))
            .ToList();

        var response = new ListTradesResponse(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount
        );

        return Result<ListTradesResponse>.Success(response);
    }
}
