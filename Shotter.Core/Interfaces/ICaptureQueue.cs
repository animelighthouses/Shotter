using Shotter.Core.Models;

namespace Shotter.Core.Interfaces;

public interface ICaptureQueue
{
    bool TryEnqueue(CaptureJob job);

    ValueTask<CaptureJob> DequeueAsync(
        CancellationToken cancellationToken);

    bool IsProcessing { get; }

    void SetProcessing(bool processing);
}