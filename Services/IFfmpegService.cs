namespace Shotter.Services;

public interface IFfmpegService
{
    Task TakeScreenshot(CancellationToken cancellationToken, double positionSeconds,
        string mediaPath,
        string outputPath,
        bool includeSubtitles);
}