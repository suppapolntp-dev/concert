namespace TicketWorld.WebAPI.Middlewares;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// RFC 7807 compliant global exception handler middleware translating domain exceptions to HTTP status codes.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        var statusCode = HttpStatusCode.InternalServerError;
        var title = "An unexpected error occurred.";
        var detail = exception.Message;

        switch (exception)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Resource Not Found";
                break;

            case DbUpdateConcurrencyException:
                statusCode = HttpStatusCode.Conflict;
                title = "Concurrency Conflict";
                detail = "The requested resource was modified by another concurrent operation. Please refresh and try again.";
                break;

            case ConflictException:
                statusCode = HttpStatusCode.Conflict;
                title = "Conflict";
                break;

            case ForbiddenException:
                statusCode = HttpStatusCode.Forbidden;
                title = "Forbidden";
                break;

            case BusinessRuleException:
            case DomainException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Bad Request";
                break;

            default:
                _logger.LogError(exception, "Unhandled system exception");
                detail = "An internal server error occurred.";
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
