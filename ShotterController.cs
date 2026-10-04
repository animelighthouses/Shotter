using Shotter.Services;

namespace Shotter;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class ShotterController : ControllerBase
{
    private readonly ILogger<ShotterController> _logger;
    private const string ScreenshotDirectory = "/screenshots";
    private const string ShowsDirectory = "shows";
    private const string MoviesDirectory = "movies";

    private readonly IFfmpegService _ffmpegService;
    private readonly IJellyfinService _jellyfinService;

    public ShotterController(IFfmpegService ffmpegService, IJellyfinService jellyfinService, ILogger<ShotterController> logger)
    {
        _ffmpegService = ffmpegService;
        _jellyfinService = jellyfinService;
        _logger = logger;
    }

    [HttpGet("screenshot")]
    public async Task<IActionResult> ScreenshotCurrentStream([FromQuery] bool includeSubtitles,
        CancellationToken cancellationToken)
    {
        JellyfinMediaInfo mediaInfo;
        try
        {
            mediaInfo= await _jellyfinService.GetPlaybackInformation(cancellationToken);
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

        var outputPath = ResolveOutputPath(mediaInfo, out var episodeTimestamp);

        _logger.LogInformation(
            "Taking a screenshot of {MediaInfoSeriesName} S{MediaInfoParentIndexNumber:D2}E{MediaInfoIndexNumber:D2} at {EpisodeTimestamp} with subtitles {IncludeSubtitles}", 
            mediaInfo.SeriesName, 
            mediaInfo.ParentIndexNumber, 
            mediaInfo.IndexNumber, 
            episodeTimestamp, 
            includeSubtitles);

        var (exitCode, stderr) = await _ffmpegService.TakeScreenshot(
            cancellationToken, 
            mediaInfo.PositionSeconds, 
            mediaInfo.MediaPath, 
            outputPath,
            includeSubtitles);

        if (exitCode != 0 || !System.IO.File.Exists(outputPath))
        {
            return StatusCode(
                500,
                new
                {
                    error = "ffmpeg failed to create the screenshot.",
                    exitCode,
                    details = stderr
                });
        }

        return Ok(new
        {
            file = outputPath,
        });
    }

    private string ResolveOutputPath(JellyfinMediaInfo mediaInfo, out string videoTimeStamp)
    {
        videoTimeStamp = GetVideoTimestamp(mediaInfo.PositionSeconds);
        
        // TODO: filenames should escape all filesystem unfriendly characters.
        if (mediaInfo.IsMovie)
        {
            var movieNamePath = mediaInfo.Name!.Replace(" ", "_");
            var movieDirectory = Path.Combine(ScreenshotDirectory, MoviesDirectory, movieNamePath);
            Directory.CreateDirectory(movieDirectory);
            return Path.Combine(
                movieDirectory,
                $"{movieNamePath}_{videoTimeStamp}.jpg");
        }
        var seriesNamePath = mediaInfo.SeriesName!.Replace(" ", "_");
        var seriesDirectory = Path.Combine(ScreenshotDirectory, ShowsDirectory, seriesNamePath);
        
        Directory.CreateDirectory(seriesDirectory);
        
        var outputPath = Path.Combine(
            seriesDirectory,
            $"{seriesNamePath}_S{mediaInfo.ParentIndexNumber:D2}E{mediaInfo.IndexNumber:D2}_{videoTimeStamp}.jpg");
        return outputPath;
    }


    private static string GetVideoTimestamp(double positionSeconds)
    {
        var time = TimeSpan.FromSeconds(positionSeconds);

        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours:D2}h{time.Minutes:D2}m{time.Seconds:D2}s"
            : $"{time.Minutes:D2}m{time.Seconds:D2}s";
    }
}

