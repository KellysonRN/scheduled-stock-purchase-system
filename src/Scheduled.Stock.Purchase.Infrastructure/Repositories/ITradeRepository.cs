using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Infrastructure.Repositories;

public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(TradeId tradeId, CancellationToken cancellationToken = default);

    Task<List<Trade>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Trade trade, CancellationToken cancellationToken = default);

    Task UpdateAsync(Trade trade, CancellationToken cancellationToken = default);

    Task DeleteAsync(TradeId tradeId, CancellationToken cancellationToken = default);
}
