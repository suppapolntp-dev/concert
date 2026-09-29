namespace TicketWorld.Application.Common.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Pipeline behavior logging MediatR request execution and completion.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing MediatR request {RequestName}", requestName);

        var response = await next();

        _logger.LogInformation("Completed MediatR request {RequestName}", requestName);
        return response;
    }
}
