using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Domain.ValueObjects;

public class ContributionAmountErrors
{
    public static readonly Error ZeroContribution = new(
        "ContributionAmount.Zero",
        "Contribution must be greater than zero"
    );
}
