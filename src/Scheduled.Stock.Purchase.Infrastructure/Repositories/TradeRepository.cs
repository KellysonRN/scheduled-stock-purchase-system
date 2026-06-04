using Microsoft.EntityFrameworkCore;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Infrastructure.Data;

namespace Scheduled.Stock.Purchase.Infrastructure.Repositories;

public sealed class TradeRepository : ITradeRepository
{
    private readonly ScheduledStockPurchaseDbContext _dbContext;

    public TradeRepository(ScheduledStockPurchaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Trade?> GetByIdAsync(
        TradeId tradeId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Trades.FirstOrDefaultAsync(t => t.Id == tradeId, cancellationToken);
    }

    public async Task<List<Trade>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1)
            pageSize = 10;

        return await _dbContext
            .Trades.OrderByDescending(t => t.ExecutedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trades.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        await _dbContext.Trades.AddAsync(trade, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        _dbContext.Trades.Update(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TradeId tradeId, CancellationToken cancellationToken = default)
    {
        var trade = await GetByIdAsync(tradeId, cancellationToken);
        if (trade is null)
            return;

        _dbContext.Trades.Remove(trade);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
