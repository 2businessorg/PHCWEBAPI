using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;
using System.Diagnostics;
using Shared.Kernel.Responses;
using Shared.Kernel.Extensions;

namespace PHCAPI.Host.Middleware;

/// <summary>
/// Middleware centralizado para tratamento de exceções usando ResponseDTO
/// Trabalha em conjunto com ResponseLoggingMiddleware para auditoria completa
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.GetCorrelationId();
        var operation = $"{httpContext.Request.Method} {httpContext.Request.Path}";

        _logger.LogError(
            exception,
            "Exception occurred: {Message} | Operation: {Operation} | CorrelationId: {CorrelationId}",
            exception.Message,
            operation,
            correlationId);

        // ✅ Criar response apropriada
        var (response, statusCode) = CreateResponse(exception, httpContext);

        // ⚠️ NOTA: Logging via Audit será feito pelo ResponseLoggingMiddleware
        // que intercepta a response APÓS este handler
        // Não precisamos logar aqui para evitar duplicação

        // ✅ Retornar resposta ao cliente
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (ResponseDTO response, int statusCode) CreateResponse(Exception exception, HttpContext context)
    {
        var error = new 
        {
            message = exception?.Message,
            stack = exception?.StackTrace?.ToString(),
            inner = exception?.InnerException?.ToString()
        };

        Debug.Print($"INTERNAL ERROR ON {context.Request.Path}: {error.message}\nStack Trace: {error.stack}\nInner Exception: {error.inner}");

        // Obter Correlation ID do HttpContext
        var correlationId = context.GetCorrelationId();

        return exception switch
        {
            ValidationException validationException => (
                ResponseDTO.Error(
                    ResponseCodes.ValidationError,
                    data: new
                    {
                        errors = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                    },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status400BadRequest
            ),

            KeyNotFoundException notFoundException => (
                ResponseDTO.Error(
                    ResponseCodes.NotFound,
                    data: new { message = notFoundException.Message },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status404NotFound
            ),

            ArgumentException argumentException => (
                ResponseDTO.Error(
                    ResponseCodes.ValidationError,
                    data: new { message = argumentException.Message },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status400BadRequest
            ),

            UnauthorizedAccessException => (
                ResponseDTO.Error(
                    ResponseCodes.Unauthorized,
                    data: new { message = "Authentication is required to access this resource." },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status401Unauthorized
            ),

            InvalidOperationException invalidOperationException => (
                ResponseDTO.Error(
                    ResponseCodes.ValidationError,
                    data: new { message = invalidOperationException.Message },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status400BadRequest
            ),

            _ => (
                ResponseDTO.Error(
                    ResponseCodes.InternalError,
                    data: new { message = "An unexpected error occurred. Please try again later." },
                    content: new { path = context.Request.Path.ToString() },
                    correlationId: correlationId
                ),
                StatusCodes.Status500InternalServerError
            )
        };
    }
}
