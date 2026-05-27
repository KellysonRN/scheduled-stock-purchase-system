using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Api.Constants;
using Scheduled.Stock.Purchase.Api.Extensions;
using Scheduled.Stock.Purchase.Shared;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.CreateTrade;

internal sealed class CreateTradeEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/trades", async (
                IHandler<CreateTradeRequest, Result<CreateTradeResponse>> handler,
                CreateTradeRequest command,
                CancellationToken cancellationToken
            ) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult(trade => Results.Created($"/trades/{trade.Id}", trade));
        })
        .WithTags(ApiTags.Trades)
        .Produces<CreateTradeResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
