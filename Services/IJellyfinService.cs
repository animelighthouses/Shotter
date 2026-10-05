namespace Shotter.Services;

public interface IJellyfinService
{
    Task<JellyfinMediaInfo> GetPlaybackInformation(CancellationToken cancellationToken);
}

public class JellyfinMediaInfo
{
    /// <summary>
    /// Name of the episode or the movie.
    /// </summary>
    public string? Name { get; set; }
    public string? SeriesName { get; set; }
    public int? ParentIndexNumber { get; set; }
    public int? IndexNumber { get; set; }
    public bool IsMovie { get; set; }
    public double PositionSeconds { get; set; }
    public string MediaPath { get; set; }
    public int? SubtitlesIndex { get; set; }
    public string? SubtitlesCodec { get; set; }
    public string? ExternalSubtitlePath { get; set; }
}