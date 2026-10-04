namespace Shotter.Services;

public sealed class ScreenshotBackgroundService : BackgroundService
{
    private readonly IScreenshotQueue _queue;
    private readonly ILogger<ScreenshotBackgroundService> _logger;
    private readonly IFfmpegService _ffmpegService;

    public ScreenshotBackgroundService(
        IScreenshotQueue queue,
        ILogger<ScreenshotBackgroundService> logger, 
        IFfmpegService ffmpegService)
    {
        _queue = queue;
        _logger = logger;
        _ffmpegService = ffmpegService;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                _logger.LogInformation(
                    "Processing screenshot job for {MediaPath} at {PositionSeconds}s with subtitles {IncludeSubtitles}",
                    job.MediaPath,
                    job.PositionSeconds,
                    job.IncludeSubtitles);

                await ProcessAsync(job, stoppingToken);
                
                _logger.LogInformation(
                    "Finished processing screenshot job for {MediaPath} at {PositionSeconds}s with subtitles {IncludeSubtitles}",
                    job.MediaPath,
                    job.PositionSeconds,
                    job.IncludeSubtitles);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Screenshot job failed");
            }
        }
    }

    private async Task ProcessAsync(
        ScreenshotJob job,
        CancellationToken cancellationToken)
    {
        await _ffmpegService.TakeScreenshot(
            cancellationToken,
            job.PositionSeconds,
            job.MediaPath,
            job.OutputPath,
            job.IncludeSubtitles);
    }
}
