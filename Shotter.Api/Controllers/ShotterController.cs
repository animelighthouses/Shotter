using Microsoft.AspNetCore.Mvc;
using Shotter.Core.Exceptions;
using Shotter.Core.Interfaces;
using Shotter.Core.Models;
using Shotter.Services.Notifications;

namespace Shotter.Api.Controllers;

[ApiController]
[Route("api")]
public class ShotterController(
    ICaptureQueue captureQueue,
    INotificationService notificationService,
    IScreenshotService screenshotService)
    : ControllerBase
{
    [HttpGet("screenshot")]
    public async Task<IActionResult> ScreenshotCurrentStream([FromQuery] bool includeSubtitles,
        CancellationToken cancellationToken)
    {

        try
        {
            await screenshotService.HandleScreenshotRequest(includeSubtitles, cancellationToken);
        }
        catch (CaptureQueueFullException exception)
        {
            return StatusCode( 
                StatusCodes.Status429TooManyRequests,
                new { error = exception.Message });
        }
        catch (Exception exception)
        {
            return StatusCode(
                500,
                new
                {
                    error = exception.Message
                });
        }
        
        return Accepted();
    }

    [HttpGet("is-processing")]
    public IActionResult IsProcessingScreenshots()
    {
        return Ok(captureQueue.IsProcessing);
    }

    [HttpGet("test-notification")]
    public IActionResult TestNotification()
    {
        if (notificationService is NoopNotificationService)
        {
            return BadRequest("You have not configured a notification service.");
        }
        notificationService.SendNotification("This is a test notification.", CancellationToken.None);
        return Accepted();
    }
}

