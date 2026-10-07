using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Shotter.Core.Configuration;
using Shotter.Core.Interfaces;
using Shotter.Core.Models;

namespace Shotter.Services.Screenshot;

public sealed class ScreenshotQueue : IScreenshotQueue
{
    private readonly Channel<ScreenshotJob> _channel;
    
    private int _processing;
    public bool IsProcessing => Volatile.Read(ref _processing) == 1;

    public void SetProcessing(bool processing)
    {
        Interlocked.Exchange(ref _processing, processing ? 1 : 0);
    }
    
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
