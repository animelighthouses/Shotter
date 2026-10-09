namespace Shotter.Core.Exceptions;

public class PlaybackProviderConnectionException(string message, Exception? exception = null) : Exception(message, exception);