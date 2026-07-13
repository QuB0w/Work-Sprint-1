using System.Net;
using System.Text.Json;
using Sprint_1_WebAPI.Exceptions;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An internal server error occurred."
        };

        switch (exception)
        {
            case NoAvailableSeatsException noSeatsEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = noSeatsEx.Message;
                break;
            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argEx.Message;
                break;
            case KeyNotFoundException keyEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = keyEx.Message;
                break;
            case InvalidOperationException opEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = opEx.Message;
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred.";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
