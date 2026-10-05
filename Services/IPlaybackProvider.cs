namespace Shotter.Services;

public interface IPlaybackProvider
{
    Task<CurrentPlayback> GetPlaybackInformation(CancellationToken cancellationToken);
}

public class CurrentPlayback
{
    /// <summary>
    /// Name of the episode or the movie.
    /// </summary>
    public string? Name { get; set; }
    public string? SeriesName { get; set; }
    
    /// <summary>
    /// Season number.
    /// </summary>
    public int? ParentIndexNumber { get; set; }
    
    /// <summary>
    /// Episode number.
    /// </summary>
    public int? IndexNumber { get; set; }
    public bool IsMovie { get; set; }
    
    /// <summary>
    /// Playback position in seconds.
    /// </summary>
    public double PositionSeconds { get; set; }
    
    /// <summary>
    /// Path to the played media on disc.
    /// </summary>
    public string MediaPath { get; set; }
    
    /// <summary>
    /// Index of the subtitle track.
    /// </summary>
    public int? SubtitlesIndex { get; set; }
    
    /// <summary>
    /// Codec of the subtitle track. E.g. ASS or SRT.
    /// </summary>
    public string? SubtitlesCodec { get; set; }
    
    /// <summary>
    /// Path to external subtitles on disc.
    /// </summary>
    public string? ExternalSubtitlePath { get; set; }
}