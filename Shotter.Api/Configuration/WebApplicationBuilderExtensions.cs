using Shotter.Core.Configuration;
using Shotter.Core.Enums;
using Shotter.Core.Interfaces;
using Shotter.Services.Notifications;
using Shotter.Services.Playback;

namespace Shotter.Api.Configuration;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder RegisterConfigurations()
        {
            builder.Services.Configure<ShotterOptions>(builder.Configuration);
            builder.Services.AddOptions<NtfyOptions>().BindConfiguration("Ntfy");
            builder.Services.AddOptions<FfmpegOptions>().BindConfiguration("Ffmpeg");
            return builder;
        }
        
        public WebApplicationBuilder RegisterPlaybackProvider()
        {
            if (builder.Configuration.GetValue<MediaServerType>(nameof(ShotterOptions.MediaServerType)) == MediaServerType.Jellyfin)
            {
                builder.Services.AddTransient<IPlaybackProvider, JellyfinPlaybackProvider>();
            }
            else
            {
                builder.Services.AddTransient<IPlaybackProvider, PlexPlaybackProvider>();
            }
        
            return builder;
        }

        public WebApplicationBuilder RegisterNotificationProvider()
        {
            if (builder.Configuration.GetValue<NotificationProviderType?>(nameof(ShotterOptions.NotificationProvider)) == NotificationProviderType.Ntfy)
            {
                builder.Services.AddTransient<INotificationService, NtfyService>();
            }
            else
            {
                builder.Services.AddTransient<INotificationService, NoopNotificationService>();
            }

            return builder;
        }
    }
}