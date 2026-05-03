using System.Text.RegularExpressions;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed class Ticker : IEquatable<Ticker>
{
    public string Value { get; }

    private Ticker(string value)
    {
        Value = value;
    }

    public static Result<Ticker> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Ticker>.Failure(TickerErrors.Required);

        var span = value.AsSpan().Trim();

        var normalized = span.ToString().ToUpperInvariant();

        return !Regex.IsMatch(normalized, @"^[A-Z]{4}[0-9]{1,2}$")
            ? Result<Ticker>.Failure(TickerErrors.InvalidFormat)
            : Result<Ticker>.Success(new Ticker(normalized));
    }

    public bool Equals(Ticker? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}
