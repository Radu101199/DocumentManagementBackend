using System.Net;
using System.Text.Json;
using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Domain.Exceptions;
using FluentValidation;

namespace DocumentManagementBackend.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage })
                    .ToList<object>()
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                (List<object>?)null
            ),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                (List<object>?)null
            ),
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                domainEx.Message,
                (List<object>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An error occurred while processing your request",
                (List<object>?)null
            )
        };

        // ✅ Structured logging cu context
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["StatusCode"] = (int)statusCode,
            ["ExceptionType"] = exception.GetType().Name
        }))
        {
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                // 500 → log complet cu stack trace
                _logger.LogError(exception,
                    "Unhandled exception on {Method} {Path}: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
            }
            else
            {
                // 4xx → log warning fără stack trace
                _logger.LogWarning(
                    "Handled exception {ExceptionType} on {Method} {Path}: {Message}",
                    exception.GetType().Name,
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
            }
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors,
            traceId = context.TraceIdentifier,
            // TEMPORAR pentru debug:
            // detail = exception.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}