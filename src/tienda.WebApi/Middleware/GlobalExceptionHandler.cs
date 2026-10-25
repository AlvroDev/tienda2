using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using tienda.Application.Common.Exceptions;
using tienda.Domain.Exceptions;

namespace tienda.WebApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. Loguear siempre, con el stacktrace completo (solo en el servidor, nunca al cliente)
        _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);

        // 2. Validación: 400 con la lista de errores por campo
        if (exception is ValidationException validationEx)
        {
            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validación",
                    Instance = httpContext.Request.Path
                },
                (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }

        // 3. El resto: mapear tipo de excepción → código HTTP
        //    NotFoundException va ANTES que DomainException porque hereda de ella
        var (statusCode, title) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            InvalidCredentialsException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = statusCode, Title = title, Instance = httpContext.Request.Path },
            (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);

        // 4. true = "ya lo manejé", el framework no sigue buscando otro handler
        return true;
    }
}