using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Api.Constants;
using Scheduled.Stock.Purchase.Api.Extensions;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.DeleteTrade;

internal sealed class DeleteTradeEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/trades/{id}", HandleAsync)
            .WithName("DeleteTrade")
            .WithTags(ApiTags.Trades)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IHandler<DeleteTradeRequest, Result<DeleteTradeResponse>> handler,
        CancellationToken cancellationToken
    )
    {
        var request = new DeleteTradeRequest(id);
        var result = await handler.HandleAsync(request, cancellationToken);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}