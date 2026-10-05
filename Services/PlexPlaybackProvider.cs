namespace Shotter.Services;

public class PlexPlaybackProvider : IPlaybackProvider
{
    public Task<CurrentPlayback> GetPlaybackInformation(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}