using Shotter.Core.Models;

namespace Shotter.Core.Interfaces;

public interface IFileNameResolver
{
    (string outputDirectory, string outputFile) ResolveOutputPath(CurrentPlayback mediaInfo);
}