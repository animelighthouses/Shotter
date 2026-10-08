using Shotter.Core.Configuration;
using Shotter.Core.Enums;
using Shotter.Core.Interfaces;
using Shotter.Services;
using Shotter.Services.Notifications;
using Shotter.Services.Playback;
using Shotter.Services.Screenshot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ShotterOptions>(builder.Configuration);
builder.Services.AddOptions<NtfyOptions>().BindConfiguration("Ntfy");

builder.Services.AddSingleton<ICaptureQueue, CaptureQueue>();
builder.Services.AddHostedService<ScreenshotBackgroundService>();

if (builder.Configuration.GetValue<MediaServerType>(nameof(ShotterOptions.MediaServerType)) == MediaServerType.Jellyfin)
{
    builder.Services.AddTransient<IPlaybackProvider, JellyfinPlaybackProvider>();
}
else
{
    builder.Services.AddTransient<IPlaybackProvider, PlexPlaybackProvider>();
}

if (builder.Configuration.GetValue<NotificationProviderType?>(nameof(ShotterOptions.NotificationProvider)) == NotificationProviderType.Ntfy)
{
    builder.Services.AddTransient<INotificationService, NtfyService>();
}
else
{
    builder.Services.AddTransient<INotificationService, NoopNotificationService>();
}

builder.Services
    .AddTransient<IScreenshotService, ScreenshotService>()
    .AddTransient<IScreenshotProcessor, ScreenshotProcessor>()
    .AddTransient<IFileNameResolver, FileNameResolver>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapControllers();

app.Run();