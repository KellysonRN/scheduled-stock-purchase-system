using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.Entities;

public sealed class Client : Entity<Guid>
{
    public string FullName { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public Cpf Cpf { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    private Client() { }

    private Client(Guid id, string fullName, string email, Cpf cpf, DateTime createdAt)
        : base(id)
    {
        FullName = fullName;
        Email = email;
        Cpf = cpf;
        CreatedAt = createdAt;
    }

    public static Result<Client> Create(string? fullName, string? email, Cpf? cpf)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result<Client>.Failure(ClientErrors.InvalidFullName);

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return Result<Client>.Failure(ClientErrors.InvalidEmail);

        if (cpf is null)
            return Result<Client>.Failure(ClientErrors.InvalidCpf);

        var client = new Client(
            Guid.NewGuid(),
            fullName.Trim(),
            email.Trim(),
            cpf,
            DateTime.UtcNow
        );
        return Result<Client>.Success(client);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
