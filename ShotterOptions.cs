namespace Shotter;

public class ShotterOptions
{
    public string MediaServerUrl { get; set; }
    public string MediaServerApiKey { get; set; }
    public string MediaServerUserId { get; set; }
    public MediaServerType MediaServerType { get; set; } = MediaServerType.Jellyfin;
    public int ScreenshotQueueCapacity { get; set; } = 20;
}