namespace Shotter.Services;

public sealed partial class ScreenshotBackgroundService : BackgroundService
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
                _queue.SetProcessing(true);

                LogProcesssing(job.MediaPath, job.PositionSeconds, job.IncludeSubtitles, job.SubtitlesCodec,
                    job.SubtitleIndex, job.ExternalSubtitlePath, job.OutputPath);

                await ProcessAsync(job, stoppingToken);
                LogProcessingFinished(job.MediaPath, job.PositionSeconds);
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
            finally
            {
                _queue.SetProcessing(false);
            }
        }
    }

    private async Task ProcessAsync(
        ScreenshotJob job,
        CancellationToken cancellationToken)
    {
        // Burning subtitles is a longrunning process, so if something deletes the directory while jobs are still
        // in queue, they will fail because of the missing directory.
        Directory.CreateDirectory(job.OutPutDirectory);
        await _ffmpegService.TakeScreenshot(
            cancellationToken,
            job.PositionSeconds,
            job.MediaPath,
            job.OutputPath,
            job.IncludeSubtitles,
            job.SubtitleIndex,
            job.SubtitlesCodec,
            job.ExternalSubtitlePath);
    }

    [LoggerMessage(LogLevel.Information, "Processing screenshot job \n" +
                                         "media path:{MediaPath} \n" +
                                         "timestamp: {PositionSeconds}s \n" +
                                         "subtitles: {IncludeSubtitles} \n" +
                                         "subtitles codec:{SubtitlesCodec} \n" +
                                         "subtitles index: {SubtitlesIndex} \n" +
                                         "external subtitle path: {ExternalSubtitlePath} \n" +
                                         "output path: {OutputPath}")]
    partial void LogProcesssing(string mediaPath, double positionSeconds, bool includeSubtitles, string? subtitlesCodec,
        int? subtitlesIndex, string? externalSubtitlePath, string? outputPath);

    [LoggerMessage(LogLevel.Information, "Finished processing screenshot job for {MediaPath} at {PositionSeconds}s")]
    partial void LogProcessingFinished(string mediaPath, double positionSeconds);
}