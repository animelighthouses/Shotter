namespace Shotter;

public class ShotterOptions
{
    public string JellyfinUrl { get; set; }
    public string JellyfinApiKey { get; set; }
    public string JellyfinUserId { get; set; }
    public int ScreenshotQueueCapacity { get; set; } = 20;
}