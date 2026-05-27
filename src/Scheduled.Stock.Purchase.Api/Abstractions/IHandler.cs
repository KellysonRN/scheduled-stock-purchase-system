namespace Scheduled.Stock.Purchase.Api.Abstractions;

internal interface IHandler<in TRequest, TResult>
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
