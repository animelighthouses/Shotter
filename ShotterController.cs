using Shotter.Services;

namespace Shotter;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class ShotterController : ControllerBase
{
    private readonly ILogger<ShotterController> _logger;

    private readonly IPlaybackProvider _playbackProvider;
    private readonly IScreenshotQueue _screenshotQueue;
    private readonly IFileNameResolver _fileNameResolver;

    public ShotterController(
        IPlaybackProvider playbackProvider, 
        ILogger<ShotterController> logger, 
        IScreenshotQueue screenshotQueue, IFileNameResolver fileNameResolver)
    {
        _playbackProvider = playbackProvider;
        _logger = logger;
        _screenshotQueue = screenshotQueue;
        _fileNameResolver = fileNameResolver;
    }

    [HttpGet("screenshot")]
    public async Task<IActionResult> ScreenshotCurrentStream([FromQuery] bool includeSubtitles,
        CancellationToken cancellationToken)
    {
        CurrentPlayback mediaInfo;
        try
        {
            mediaInfo= await _playbackProvider.GetPlaybackInformation(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to query media info.");
            return StatusCode(
                500,
                new
                {
                    error = exception.Message
                });
        }

        var output = _fileNameResolver.ResolveOutputPath(mediaInfo);
        
        var job = new ScreenshotJob(
            mediaInfo.MediaPath,
            mediaInfo.PositionSeconds,
            includeSubtitles,
            mediaInfo.SubtitlesIndex,
            mediaInfo.SubtitlesCodec,
            mediaInfo.ExternalSubtitlePath,
            Path.Combine(output.outputDirectory, output.outputFile),
            output.outputDirectory);
        
        _logger.LogInformation(
            "Queuing screenshot job for {MediaPath} at {PositionSeconds}s with subtitles {IncludeSubtitles}",
            job.MediaPath,
            job.PositionSeconds,
            job.IncludeSubtitles);
        
        if (_screenshotQueue.TryEnqueue(job))
        {
            return Accepted(new
            {
                message = "Screenshot queued."
            });
        }
        
        _logger.LogWarning("Screenshot queue is full.");
        return StatusCode( 
            StatusCodes.Status429TooManyRequests,
            new { error = "Screenshot queue is full" });

    }

    [HttpGet("is-processing")]
    public IActionResult IsProcessingScreenshots()
    {
        return Ok(_screenshotQueue.IsProcessing);
    }
}

