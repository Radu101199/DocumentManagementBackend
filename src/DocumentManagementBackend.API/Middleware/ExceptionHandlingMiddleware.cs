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
                validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList()
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                null
            ),
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                domainEx.Message,
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An error occurred while processing your request",
                null
            )
        };

        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}