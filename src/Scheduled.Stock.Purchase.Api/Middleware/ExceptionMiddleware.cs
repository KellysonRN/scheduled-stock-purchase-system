using System.Net.Mime;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Scheduled.Stock.Purchase.Api.Middleware;

/// <summary>
/// Middleware responsável pelo tratamento global de exceções não tratadas na aplicação.
/// Implementa a interface <see cref="IExceptionHandler"/> para interceptar erros e retornar um <see cref="ProblemDetails"/> apropriado.
/// </summary>
public sealed class ExceptionMiddleware : IExceptionHandler
{
    /// <summary>
    /// Intercepta a exceção lançada, configura o <see cref="ProblemDetails"/> conforme o tipo de erro e escreve a resposta JSON no <see cref="HttpContext.Response"/>.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP da requisição.</param>
    /// <param name="exception">Exceção capturada a ser tratada.</param>
    /// <param name="cancellationToken">Token para cancelar a operação, se necessário.</param>
    /// <returns>
    /// <c>true</c> se a exceção foi tratada e a resposta escrita; caso contrário, <c>false</c>.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = new();

        switch (exception)
        {
            case NotImplementedException:
                problem.Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-501-not-implemented";
                problem.Status = StatusCodes.Status501NotImplemented;
                problem.Title = "Este recurso ainda não está disponível.";
                break;

            default:
                problem.Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error";
                problem.Status = StatusCodes.Status500InternalServerError;
                problem.Title = "Erro Interno do Servidor. Por favor, tente novamente mais tarde.";
                break;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}