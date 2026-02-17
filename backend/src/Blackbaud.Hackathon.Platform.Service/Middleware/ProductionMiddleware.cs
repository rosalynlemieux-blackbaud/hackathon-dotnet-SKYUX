using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Blackbaud.Hackathon.Platform.Service.Middleware;

/// <summary>
/// Global exception handling middleware for production error handling
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        // Log the exception with correlation ID
        var correlationId = context.TraceIdentifier;
        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId,
            context.Request.Path);

        // Determine status code and error message
        var (statusCode, errorMessage, errorType) = GetErrorDetails(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = errorMessage,
            Type = errorType,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        // Include stack trace in development only
        if (_env.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode statusCode, string message, string type) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message, "ValidationError"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "Required parameter is missing", "ValidationError"),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message, "InvalidOperation"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access", "AuthorizationError"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found", "NotFoundError"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Feature not implemented", "NotImplemented"),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Request timeout", "TimeoutError"),
            _ => (HttpStatusCode.InternalServerError, "An error occurred processing your request", "ServerError")
        };
    }
}

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// Request/Response logging middleware for audit trails
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        var correlationId = context.TraceIdentifier;
        var userId = context.User?.FindFirst("sub")?.Value ?? "Anonymous";
        
        _logger.LogInformation(
            "HTTP {Method} {Path} started. User: {UserId}, CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            userId,
            correlationId);

        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);

            // Log response
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "HTTP {Method} {Path} completed with {StatusCode} in {Duration}ms. User: {UserId}, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds,
                userId,
                correlationId);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex,
                "HTTP {Method} {Path} failed after {Duration}ms. User: {UserId}, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                duration.TotalMilliseconds,
                userId,
                correlationId);
            throw;
        }
    }
}

/// <summary>
/// Security headers middleware for production hardening
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security headers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        
        // Content Security Policy (adjust based on your needs)
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'");

        // Strict Transport Security (HSTS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}

/// <summary>
/// Correlation ID middleware for request tracking
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                          ?? context.TraceIdentifier;

        // Set it on the response
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        // Store in HttpContext for later use
        context.Items["CorrelationId"] = correlationId;

        await _next(context);
    }
}
