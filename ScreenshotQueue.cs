using Microsoft.Extensions.Options;

namespace Shotter;
using System.Threading.Channels;

public sealed record ScreenshotJob(
    string MediaPath,
    double PositionSeconds,
    bool IncludeSubtitles,
    int? SubtitleIndex,
    string? SubtitlesCodec,
    string OutputPath,
    string OutPutDirectory);

public interface IScreenshotQueue
{
    bool TryEnqueue(ScreenshotJob job);

    ValueTask<ScreenshotJob> DequeueAsync(
        CancellationToken cancellationToken);

    int Count { get; }
}

public sealed class ScreenshotQueue : IScreenshotQueue
{
    private readonly Channel<ScreenshotJob> _channel;
    
    public ScreenshotQueue(IOptions<ShotterOptions> options)
    {
        _channel = Channel.CreateBounded<ScreenshotJob>(
            new BoundedChannelOptions(options.Value.ScreenshotQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false
            });
    }
    
    public int Count => _channel.Reader.Count;

    public bool TryEnqueue(ScreenshotJob job)
    {
        return _channel.Writer.TryWrite(job);
    }

    public ValueTask<ScreenshotJob> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
