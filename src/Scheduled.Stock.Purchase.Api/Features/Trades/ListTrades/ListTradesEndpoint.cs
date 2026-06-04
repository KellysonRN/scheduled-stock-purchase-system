using Scheduled.Stock.Purchase.Api.Abstractions;
using Scheduled.Stock.Purchase.Api.Constants;
using Scheduled.Stock.Purchase.Api.Extensions;

namespace Scheduled.Stock.Purchase.Api.Features.Trades.ListTrades;

internal sealed class ListTradesEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/trades", HandleAsync)
            .WithName("ListTrades")
            .WithTags(ApiTags.Trades)
            .Produces<ListTradesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync(
        int pageNumber = 1,
        int pageSize = 10,
        IHandler<ListTradesRequest, Shared.Result<ListTradesResponse>>? handler = null,
        CancellationToken cancellationToken = default
    )
    {
        if (handler is null)
            return Results.Problem(
                "Handler not found",
                statusCode: StatusCodes.Status500InternalServerError
            );

        var request = new ListTradesRequest(pageNumber, pageSize);
        var result = await handler.HandleAsync(request, cancellationToken);

        return result.ToHttpResult(response => Results.Ok(response));
    }
}
