using System.Net;
using System.Text.Json;
using ProductApi.Application.DTOs;
using ProductApi.Domain.Exceptions;
using ValidationException = ProductApi.Domain.Exceptions.ValidationException;

namespace ProductApi.API.Middleware;

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
        var (statusCode, title, errors) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            ValidationException validationEx => (HttpStatusCode.BadRequest, "Validation failed.", (IDictionary<string, string[]>?)validationEx.Errors),
            ConflictException => (HttpStatusCode.Conflict, exception.Message, null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized.", null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        var response = new ErrorResponseDto
        {
            Title = title,
            Status = (int)statusCode,
            TraceId = context.TraceIdentifier,
            Errors = errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
