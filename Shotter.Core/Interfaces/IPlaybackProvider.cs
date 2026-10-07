using Shotter.Core.Models;

namespace Shotter.Core.Interfaces;

public interface IPlaybackProvider
{
    Task<CurrentPlayback> GetPlaybackInformation(CancellationToken cancellationToken);
}