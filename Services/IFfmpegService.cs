namespace Shotter.Services;

public interface IFfmpegService
{
    Task<(int exitCode, string stderr)> TakeScreenshot(CancellationToken cancellationToken, double positionSeconds,
        string mediaPath,
        string outputPath,
        bool includeSubtitles);
}