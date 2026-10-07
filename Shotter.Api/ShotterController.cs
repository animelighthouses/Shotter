using Shotter.Services;

namespace Shotter;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class ShotterController(
    IPlaybackProvider playbackProvider,
    ILogger<ShotterController> logger,
    IScreenshotQueue screenshotQueue,
    IFileNameResolver fileNameResolver,
    INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("screenshot")]
    public async Task<IActionResult> ScreenshotCurrentStream([FromQuery] bool includeSubtitles,
        CancellationToken cancellationToken)
    {
        CurrentPlayback mediaInfo;
        try
        {
            mediaInfo= await playbackProvider.GetPlaybackInformation(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query media info.");
            return StatusCode(
                500,
                new
                {
                    error = exception.Message
                });
        }

        var output = fileNameResolver.ResolveOutputPath(mediaInfo);
        
        var job = new ScreenshotJob(
            mediaInfo.MediaPath,
            mediaInfo.PositionSeconds,
            includeSubtitles,
            mediaInfo.SubtitlesIndex,
            mediaInfo.SubtitlesCodec,
            mediaInfo.ExternalSubtitlePath,
            Path.Combine(output.outputDirectory, output.outputFile),
            output.outputDirectory);
        
        logger.LogInformation(
            "Queuing screenshot job for {MediaPath} at {PositionSeconds}s with subtitles {IncludeSubtitles}",
            job.MediaPath,
            job.PositionSeconds,
            job.IncludeSubtitles);
        
        if (screenshotQueue.TryEnqueue(job))
        {
            return Accepted(new
            {
                message = "Screenshot queued."
            });
        }
        
        logger.LogWarning("Screenshot queue is full.");
        return StatusCode( 
            StatusCodes.Status429TooManyRequests,
            new { error = "Screenshot queue is full" });

    }

    [HttpGet("is-processing")]
    public IActionResult IsProcessingScreenshots()
    {
        return Ok(screenshotQueue.IsProcessing);
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

