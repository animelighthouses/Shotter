using Shotter.Core.Enums;

namespace Shotter.Core.Configuration;

public class ShotterOptions
{
    public string MediaServerUrl { get; set; }
    public string MediaServerApiKey { get; set; }
    public string? MediaServerUserName { get; set; }
    public MediaServerType MediaServerType { get; set; } = MediaServerType.Jellyfin;
    public int ScreenshotQueueCapacity { get; set; } = 20;
    public NotificationProviderType? NotificationProvider { get; set; }
}