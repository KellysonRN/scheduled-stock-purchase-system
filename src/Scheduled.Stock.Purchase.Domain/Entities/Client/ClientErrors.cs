using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.Entities;

public static class ClientErrors
{
    public static readonly Error InvalidFullName = new(
        "Client.InvalidFullName",
        "Invalid full name"
    );

    public static readonly Error InvalidEmail = new("Client.InvalidEmail", "Invalid email");

    public static readonly Error InvalidCpf = new("Client.InvalidCpf", "Invalid CPF");
}
