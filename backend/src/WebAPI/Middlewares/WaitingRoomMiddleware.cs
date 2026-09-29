namespace TicketWorld.WebAPI.Middlewares;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

public class WaitingRoomMiddleware
{
    private const string WaitingRoomTokenHeader = "X-WaitingRoom-Token";
    private readonly RequestDelegate _next;
    private readonly ILogger<WaitingRoomMiddleware> _logger;

    public WaitingRoomMiddleware(
        RequestDelegate next,
        ILogger<WaitingRoomMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer? redis = null)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Apply waiting room token validation on high-demand seat holding
        if (path.StartsWith("/api/seats/hold") && context.Request.Method == "POST")
        {
            var token = context.Request.Headers[WaitingRoomTokenHeader].FirstOrDefault();

            if (redis != null && redis.IsConnected)
            {
                var db = redis.GetDatabase();

                if (string.IsNullOrWhiteSpace(token))
                {
                    // Check if global or event waiting room is active
                    var isWaitingRoomEnabled = await db.KeyExistsAsync("config:waitingroom:active");
                    if (isWaitingRoomEnabled)
                    {
                        await WriteWaitingRoomRequiredResponseAsync(context);
                        return;
                    }
                }
                else
                {
                    var tokenKey = $"waitingroom:token:{token}";
                    var isValid = await db.KeyExistsAsync(tokenKey);
                    if (!isValid)
                    {
                        await WriteWaitingRoomRequiredResponseAsync(context);
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

    private static async Task WriteWaitingRoomRequiredResponseAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.TooManyRequests,
            Title = "Waiting Room Required",
            Detail = "High traffic queue is active for this event. Please join the virtual waiting room to receive an admission token.",
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
