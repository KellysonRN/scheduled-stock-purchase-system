using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Extensions;

internal static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}
