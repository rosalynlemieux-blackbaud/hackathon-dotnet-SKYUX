using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Blackbaud.Hackathon.Platform.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", traceId);

            var statusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Type = $"https://api.hackathon.example.com/errors/{statusCode}",
                Title = exception?.GetType().Name ?? "An error occurred",
                Status = statusCode,
                Detail = HttpContext.RequestAborted.IsCancellationRequested 
                    ? "Request was cancelled" 
                    : (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                        ? exception?.Message 
                        : "An unexpected error occurred"),
                Instance = exceptionHandlerPathFeature?.Path,
                Extensions = new Dictionary<string, object?>
                {
                    { "traceId", traceId }
                }
            };

            Response.StatusCode = statusCode;
            return new ObjectResult(problemDetails);
        }
    }
}
