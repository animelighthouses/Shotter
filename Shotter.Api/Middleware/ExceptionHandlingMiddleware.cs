using System.Net;
using Shotter.Core.Exceptions;

namespace Shotter.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (CaptureQueueFullException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (PlaybackProviderException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (PlaybackProviderConnectionException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occured." });
        }
    }
}