namespace Shotter.Core.Exceptions;

public class PlaybackProviderException(string message) : Exception("Failed to query playback info. " + message);