using CatalogApi.Models;
using System.Net;
using System.Text.Json;

namespace CatalogApi.Middleware;

/// <summary>
/// Middleware for handling exceptions and returning structured error responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = (int)HttpStatusCode.BadRequest;
                errorResponse.Title = "Bad Request";
                errorResponse.Detail = argEx.Message;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.ErrorCode = "ARGUMENT_ERROR";
                break;

            case KeyNotFoundException keyEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Status = (int)HttpStatusCode.NotFound;
                errorResponse.Title = "Not Found";
                errorResponse.Detail = keyEx.Message;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                errorResponse.ErrorCode = "NOT_FOUND";
                break;

            case UnauthorizedAccessException unauthEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Status = (int)HttpStatusCode.Unauthorized;
                errorResponse.Title = "Unauthorized";
                errorResponse.Detail = unauthEx.Message;
                errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                errorResponse.ErrorCode = "UNAUTHORIZED";
                break;

            case TimeoutException timeoutEx:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Status = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Title = "Request Timeout";
                errorResponse.Detail = "The request timed out";
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.7";
                errorResponse.ErrorCode = "TIMEOUT";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Status = (int)HttpStatusCode.InternalServerError;
                errorResponse.Title = "Internal Server Error";
                errorResponse.Detail = "An unexpected error occurred";
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                errorResponse.ErrorCode = "INTERNAL_ERROR";
                break;
        }

        errorResponse.Instance = context.Request.Path;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}