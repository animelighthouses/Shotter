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
            logger.LogError(exception, "Failed to queue capture job.");
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (PlaybackProviderException exception)
        {
            logger.LogError(exception, "Playback ");
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (PlaybackProviderConnectionException exception)
        {
            logger.LogError(exception, "Playback provider connection failed.");
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (NotificationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Failed to send notification." });
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