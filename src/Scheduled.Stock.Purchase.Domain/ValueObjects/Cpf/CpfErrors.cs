using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public static class CpfErrors
{
    public static readonly Error Empty = new("CPF.Empty", "CPF cannot be empty");

    public static readonly Error InvalidLength = new(
        "CPF.InvalidLength",
        "CPF must have 11 digits"
    );

    public static readonly Error RepeatedDigits = new(
        "CPF.RepeatedDigits",
        "CPF cannot have all digits equal"
    );

    public static readonly Error Invalid = new("CPF.Invalid", "CPF is invalid");
}
