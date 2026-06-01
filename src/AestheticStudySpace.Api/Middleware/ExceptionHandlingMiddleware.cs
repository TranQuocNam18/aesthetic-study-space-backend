using System.Net;
using System.Text.Json;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var isDevelopment = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
        
        var (statusCode, response) = exception switch
        {
            ValidationException validation => (HttpStatusCode.BadRequest, new ErrorResponse
            {
                Success = false,
                Message = validation.Message,
                Errors = validation.Errors
            }),
            NotFoundException notFound => (HttpStatusCode.NotFound, new ErrorResponse
            {
                Success = false,
                Message = notFound.Message
            }),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, new ErrorResponse
            {
                Success = false,
                Message = unauthorized.Message
            }),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, new ErrorResponse
            {
                Success = false,
                Message = forbidden.Message
            }),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, new ErrorResponse
            {
                Success = false,
                Message = "Unauthorized."
            }),
            _ => (HttpStatusCode.InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = isDevelopment ? $"Error: {exception.GetType().Name} - {exception.Message}" : "An unexpected error occurred.",
                Debug = isDevelopment ? new 
                { 
                    Exception = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                } : null
            })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private sealed class ErrorResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public IDictionary<string, string[]>? Errors { get; init; }
        public object? Debug { get; init; }
    }
}
