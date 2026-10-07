using Shotter.Core.Models;

namespace Shotter.Core.Interfaces;

public interface IScreenshotQueue
{
    bool TryEnqueue(ScreenshotJob job);

    ValueTask<ScreenshotJob> DequeueAsync(
        CancellationToken cancellationToken);

    bool IsProcessing { get; }

    void SetProcessing(bool processing);
}