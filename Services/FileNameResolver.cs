namespace Shotter.Services;

public interface IFileNameResolver
{
    (string outputDirectory, string outputFile) ResolveOutputPath(CurrentPlayback mediaInfo);
}

public class FileNameResolver : IFileNameResolver
{
    private const string ScreenshotDirectory = "/screenshots";
    private const string ShowsDirectory = "shows";
    private const string MoviesDirectory = "movies";
    
    public (string outputDirectory, string outputFile) ResolveOutputPath(CurrentPlayback mediaInfo)
    {
        var videoTimeStamp = GetVideoTimestamp(mediaInfo.PositionSeconds);
        
        // TODO: filenames should escape all filesystem unfriendly characters.
        if (mediaInfo.IsMovie)
        {
            var movieNamePath = mediaInfo.Name!.Replace(" ", "_");
            var movieDirectory = Path.Combine(ScreenshotDirectory, MoviesDirectory, movieNamePath);
            Directory.CreateDirectory(movieDirectory);
            return (movieDirectory, $"{movieNamePath}_{videoTimeStamp}.jpg");
        }
        var seriesNamePath = mediaInfo.SeriesName!.Replace(" ", "_");
        var seriesDirectory = Path.Combine(ScreenshotDirectory, ShowsDirectory, seriesNamePath);
        
        Directory.CreateDirectory(seriesDirectory);

        return (seriesDirectory,
            $"{seriesNamePath}_S{mediaInfo.ParentIndexNumber:D2}E{mediaInfo.IndexNumber:D2}_{videoTimeStamp}.jpg");
    }


    private static string GetVideoTimestamp(double positionSeconds)
    {
        var time = TimeSpan.FromSeconds(positionSeconds);

        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours:D2}h{time.Minutes:D2}m{time.Seconds:D2}s{time.Milliseconds:D3}ms"
            : $"{time.Minutes:D2}m{time.Seconds:D2}s{time.Milliseconds:D3}ms";
    }
}