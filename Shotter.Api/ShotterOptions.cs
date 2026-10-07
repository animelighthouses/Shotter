namespace Shotter;

public class ShotterOptions
{
    public string MediaServerUrl { get; set; }
    public string MediaServerApiKey { get; set; }
    public string? MediaServerUserId { get; set; }
    public MediaServerType MediaServerType { get; set; } = MediaServerType.Jellyfin;
    public int ScreenshotQueueCapacity { get; set; } = 20;
    public string? NotificationService { get; set; }
}

public class NtfyOptions
{
    public string? Url { get; set; }
    public string? Topic { get; set; }
    public string? AccessToken { get; set; }
}