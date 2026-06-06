using MediatR;
using Microsoft.Extensions.Logging;
namespace ApartmentManagementSystem.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for request logging
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request of type {RequestType}", typeof(TRequest).Name);
        
        var response = await next();
        
        _logger.LogInformation("Completed request of type {RequestType}", typeof(TRequest).Name);
        
        return response;
    }
}
