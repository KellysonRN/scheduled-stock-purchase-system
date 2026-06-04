using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Api.Constants;
using Scheduled.Stock.Purchase.Api.Extensions;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.UpdateTrade;

internal sealed class UpdateTradeEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("/trades/{id}", HandleAsync)
            .WithName("UpdateTrade")
            .WithTags(ApiTags.Trades)
            .Produces<UpdateTradeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateTradeRequest request,
        IHandler<UpdateTradeRequest, Shared.Result<UpdateTradeResponse>> handler,
        CancellationToken cancellationToken
    )
    {
        var updateRequest = request with { Id = id };
        var result = await handler.HandleAsync(updateRequest, cancellationToken);

        return result.ToHttpResult(response => Results.Ok(response));
    }
}
