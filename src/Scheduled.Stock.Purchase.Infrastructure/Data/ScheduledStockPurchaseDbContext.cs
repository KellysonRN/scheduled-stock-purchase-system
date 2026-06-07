using Microsoft.EntityFrameworkCore;
using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Infrastructure.Data;

public sealed class ScheduledStockPurchaseDbContext : DbContext
{
    public ScheduledStockPurchaseDbContext(
        DbContextOptions<ScheduledStockPurchaseDbContext> options
    )
        : base(options) { }

    public DbSet<Trade> Trades { get; set; } = null!;

    public DbSet<Client> Clients { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Trade Entity Configuration
        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasConversion(tradeId => tradeId.Value, guid => TradeId.Create(guid).Value)
                .IsRequired();

            // Ticker as owned type
            entity.OwnsOne(
                e => e.Ticker,
                o =>
                {
                    o.Property(t => t.Value).HasColumnName("Ticker").HasMaxLength(6).IsRequired();
                }
            );

            // Quantity as owned type
            entity.OwnsOne(
                e => e.Quantity,
                o =>
                {
                    o.Property(q => q.Value).HasColumnName("Quantity").IsRequired();
                }
            );

            // Money (Price) as owned type
            entity.OwnsOne(
                e => e.Price,
                o =>
                {
                    o.Property(m => m.Amount)
                        .HasColumnName("Price")
                        .HasPrecision(18, 2)
                        .IsRequired();
                }
            );

            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(10).IsRequired();

            entity.Property(e => e.ExecutedAt).IsRequired();

            entity.HasIndex(e => e.ExecutedAt);
        });

        // Client Entity Configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();

            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();

            // Cpf as owned type
            entity.OwnsOne(
                e => e.Cpf,
                o =>
                {
                    o.Property(c => c.Number).HasColumnName("Cpf").HasMaxLength(11).IsRequired();
                }
            );

            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraints
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasIndex(e => e.Cpf).IsUnique();
        });
    }
}
