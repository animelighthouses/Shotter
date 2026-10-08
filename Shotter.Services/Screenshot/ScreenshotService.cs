using Microsoft.Extensions.Logging;
using Shotter.Core.Exceptions;
using Shotter.Core.Interfaces;
using Shotter.Core.Models;

namespace Shotter.Services.Screenshot;

public class ScreenshotService(
    IPlaybackProvider playbackProvider, 
    ILogger<ScreenshotService> logger,
    ICaptureQueue captureQueue,
    IFileNameResolver fileNameResolver) : IScreenshotService
{
    public async Task HandleScreenshotRequest(
        bool includeSubtitles,
        CancellationToken cancellationToken)
    {
        CurrentPlayback mediaInfo;
        try
        {
            mediaInfo= await playbackProvider.GetPlaybackInformation(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query media info.");
            throw;
        }
        
        var output = fileNameResolver.ResolveOutputPath(mediaInfo);
        
        var job = new CaptureJob(
            mediaInfo.MediaPath,
            mediaInfo.PositionSeconds,
            includeSubtitles,
            mediaInfo.SubtitlesIndex,
            mediaInfo.SubtitlesCodec,
            mediaInfo.ExternalSubtitlePath,
            Path.Combine(output.outputDirectory, output.outputFile),
            output.outputDirectory);
        
        logger.LogInformation(
            "Queuing screenshot job for {MediaPath} at {PositionSeconds}s with subtitles {IncludeSubtitles}",
            job.MediaPath,
            job.PositionSeconds,
            job.IncludeSubtitles);
        
        if (captureQueue.TryEnqueue(job))
        {
            return;
        }
        
        logger.LogWarning("Screenshot queue is full.");
        throw new CaptureQueueFullException();
    }
}