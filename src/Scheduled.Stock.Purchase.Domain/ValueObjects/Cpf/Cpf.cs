using System.Text.RegularExpressions;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public sealed partial class Cpf : IEquatable<Cpf>
{
    public string Number { get; }

    private Cpf(string number)
    {
        Number = number;
    }

    public static Result<Cpf> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<Cpf>.Failure(CpfErrors.Empty);

        var normalized = OnlyDigits(input);

        if (normalized.Length != 11)
            return Result<Cpf>.Failure(CpfErrors.InvalidLength);

        if (HasAllDigitsEqual(normalized))
            return Result<Cpf>.Failure(CpfErrors.RepeatedDigits);

        if (!HasValidCheckDigits(normalized))
            return Result<Cpf>.Failure(CpfErrors.Invalid);

        return Result<Cpf>.Success(new Cpf(normalized));
    }

    private static string OnlyDigits(string input)
    {
        return NonDigitPattern().Replace(input, "");
    }

    private static bool HasAllDigitsEqual(string cpf)
    {
        return cpf.All(c => c == cpf[0]);
    }

    private static bool HasValidCheckDigits(string cpf)
    {
        var numbers = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        // First check digit
        var sum = 0;
        for (int i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (numbers[9] != firstDigit)
            return false;

        // Second check digit
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return numbers[10] == secondDigit;
    }

    public override string ToString()
    {
        return Convert.ToUInt64(Number).ToString(@"000\.000\.000\-00");
    }

    #region Equality

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public bool Equals(Cpf? other) => other is not null && Number == other.Number;

    public override int GetHashCode() => Number.GetHashCode();

    public static bool operator ==(Cpf left, Cpf right) => Equals(left, right);

    public static bool operator !=(Cpf left, Cpf right) => !Equals(left, right);

    #endregion

    #region Conversions

    public static implicit operator string(Cpf cpf) => cpf.Number;

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NonDigitPattern();

    #endregion
}
