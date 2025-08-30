using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Core.Logging;

public class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation($"[START] Handling {requestName}...");
        var stopwatch = Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        logger.LogInformation($"[END] {requestName} executed in {stopwatch.ElapsedMilliseconds} ms.");
        
        return response;
    }
}