using System.Diagnostics;

namespace CatalogApi.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;
        
        // Log request
        _logger.LogInformation(
            "Request started: {Method} {Path} from {RemoteIpAddress} with RequestId {RequestId}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress,
            requestId);

        // Store original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Create a new memory stream for the response body
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continue to the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response
            var responseBodyContent = await GetResponseBodyAsync(responseBody);
            
            _logger.LogInformation(
                "Request completed: {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds}ms with RequestId {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);

            // Copy the response body back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Request failed: {Method} {Path} failed after {ElapsedMilliseconds}ms with RequestId {RequestId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                requestId);

            throw;
        }
        finally
        {
            // Ensure the original response body stream is restored
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<string> GetResponseBodyAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        var body = await reader.ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        
        // Limit response body logging to prevent excessive log size
        return body.Length > 1000 ? body[..1000] + "..." : body;
    }
}