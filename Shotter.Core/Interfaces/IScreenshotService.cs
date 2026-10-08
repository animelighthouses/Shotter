namespace Shotter.Core.Interfaces;

public interface IScreenshotService
{
    Task HandleScreenshotRequest(bool includeSubtitles,
        CancellationToken cancellationToken);
}