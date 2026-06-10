using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Generate a short 8-character unique correlation ID
        string correlationId = System.Guid.NewGuid().ToString("N")[..8];

        // 2. CRITICAL: Stamp the header BEFORE calling await _next(context)
        // This ensures the header is locked into the response before downstream components start streaming data back
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // 3. Start the stopwatch and log entry
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("🛫 HTTP {Method} {Path} [Correlation ID: {CorrelationId}]", 
            context.Request.Method, context.Request.Path, correlationId);

        try
        {
            // Pass the request along to the next middleware in the pipeline
            await _next(context);
        }
        finally
        {
            // 4. Stop the stopwatch and log exit on the way back out (even if downstream threw an error)
            stopwatch.Stop();
            _logger.LogInformation("🛬 HTTP {StatusCode} finished in {ElapsedMs}ms [Correlation ID: {CorrelationId}]", 
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
        }
    }
}