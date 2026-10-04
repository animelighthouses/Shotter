namespace Shotter.Services;

public interface IFileNameResolver
{
    string ResolveOutputPath(JellyfinMediaInfo mediaInfo);
}

public class FileNameResolver : IFileNameResolver
{
    private const string ScreenshotDirectory = "/screenshots";
    private const string ShowsDirectory = "shows";
    private const string MoviesDirectory = "movies";
    
    public string ResolveOutputPath(JellyfinMediaInfo mediaInfo)
    {
        var videoTimeStamp = GetVideoTimestamp(mediaInfo.PositionSeconds);
        
        // TODO: filenames should escape all filesystem unfriendly characters.
        if (mediaInfo.IsMovie)
        {
            var movieNamePath = mediaInfo.Name!.Replace(" ", "_");
            var movieDirectory = Path.Combine(ScreenshotDirectory, MoviesDirectory, movieNamePath);
            Directory.CreateDirectory(movieDirectory);
            return Path.Combine(
                movieDirectory,
                $"{movieNamePath}_{videoTimeStamp}.jpg");
        }
        var seriesNamePath = mediaInfo.SeriesName!.Replace(" ", "_");
        var seriesDirectory = Path.Combine(ScreenshotDirectory, ShowsDirectory, seriesNamePath);
        
        Directory.CreateDirectory(seriesDirectory);
        
        var outputPath = Path.Combine(
            seriesDirectory,
            $"{seriesNamePath}_S{mediaInfo.ParentIndexNumber:D2}E{mediaInfo.IndexNumber:D2}_{videoTimeStamp}.jpg");
        return outputPath;
    }


    private static string GetVideoTimestamp(double positionSeconds)
    {
        var time = TimeSpan.FromSeconds(positionSeconds);

        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours:D2}h{time.Minutes:D2}m{time.Seconds:D2}s"
            : $"{time.Minutes:D2}m{time.Seconds:D2}s";
    }
}