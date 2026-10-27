using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace CipAdminCenter.Api;

/// <summary>Maps domain/application failures to RFC 9457 problem details. Unexpected errors are not exposed.</summary>
internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            DomainException => (StatusCodes.Status400BadRequest, "Request violates a domain rule"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = null,
            ProblemDetails = { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
