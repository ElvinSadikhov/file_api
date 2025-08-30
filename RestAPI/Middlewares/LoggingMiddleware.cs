namespace RestAPI.Middlewares;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        logger.LogInformation($"[START] {context.Request.Method} {context.Request.Path}");
        
        await next(context);

        string msg = $"[END] {context.Request.Method} {context.Request.Path} {context.Response.StatusCode}";

        if (context.Response.StatusCode >= 400)
        {
            logger.LogError(msg);
        }
        else
        {
            logger.LogInformation(msg);
        };
    }
}