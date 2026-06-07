using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Api.Constants;
using Scheduled.Stock.Purchase.Api.Extensions;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.GetTrade;

internal sealed class GetTradeEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/trades/{id}", HandleAsync)
            .WithName("GetTrade")
            .WithTags(ApiTags.Trades)
            .Produces<GetTradeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IHandler<GetTradeRequest, Shared.Result<GetTradeResponse>> handler,
        CancellationToken cancellationToken
    )
    {
        var request = new GetTradeRequest(id);
        var result = await handler.HandleAsync(request, cancellationToken);

        return result.ToHttpResult(response => Results.Ok(response));
    }
}
