using System.Net;
using System.Text.Json;
using TaskManager.Utilities.Exceptions;

public class GlobalErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlerMiddleware> _logger;

    public GlobalErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context) //se invoca en todas las peticiones
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    //Cómo le voy a responder al error
    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var statusCode = exception is BusinessException be
        ? be.StatusCode
        : StatusCodes.Status500InternalServerError;
        context.Response.StatusCode = statusCode;

        var response = new
        {
            message = "Ocurrió un error inesperado.",
            detail = exception.Message
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}