namespace Shotter.Core.Models;

public sealed record ScreenshotJob(
    string MediaPath,
    double PositionSeconds,
    bool IncludeSubtitles,
    int? SubtitleIndex,
    string? SubtitlesCodec,
    string? ExternalSubtitlePath,
    string OutputPath,
    string OutPutDirectory);