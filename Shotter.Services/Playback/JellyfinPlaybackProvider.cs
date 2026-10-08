using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shotter.Core.Configuration;
using Shotter.Core.Exceptions;
using Shotter.Core.Interfaces;
using Shotter.Core.Models;

namespace Shotter.Services.Playback;

public class JellyfinPlaybackProvider(IHttpClientFactory httpClientFactory, IOptions<ShotterOptions> options)
    : IPlaybackProvider
{
    private readonly string _jellyfinUrl = options.Value.MediaServerUrl;
    private readonly string _apiKey = options.Value.MediaServerApiKey;

    private readonly string? _userId = options.Value.MediaServerUserId;
    
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<CurrentPlayback> GetPlaybackInformation(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_jellyfinUrl}/Sessions");

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"MediaBrowser Client=\"Shotter\", " +
            $"Device=\"Server\", " +
            $"DeviceId=\"Shotter\", " +
            $"Version=\"1.0.0\", " +
            $"Token=\"{_apiKey}\"");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PlaybackProviderException("Failed to retrieve Jellyfin session.");
        }

        await using var responseStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var sessions =
            await JsonSerializer.DeserializeAsync<List<JellyfinSession>>(
                responseStream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken);

        if (sessions == null)
        {
            throw new PlaybackProviderException("Jellyfin returned no session data.");
        }

        // Find the session belonging to our hardcoded user
        // that is currently playing something.
        var session = sessions.FirstOrDefault(s =>
            (string.IsNullOrEmpty(_userId) || s.UserId == _userId) &&
            s.NowPlayingItem != null &&
            s.PlayState != null &&
            s.PlayState.PositionTicks.HasValue);

        if (session == null)
        {
            throw new PlaybackProviderException("The configured Jellyfin user is not currently playing anything.");
        }

        var itemId = session.NowPlayingItem!.Id;
        var nowPlayingItem = session.NowPlayingItem!;

        using var playbackInfoRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_jellyfinUrl}/Items/{itemId}/PlaybackInfo");

        playbackInfoRequest.Headers.TryAddWithoutValidation(
            "Authorization",
            $"MediaBrowser Client=\"Shotter\", " +
            $"Device=\"Server\", " +
            $"DeviceId=\"Shotter\", " +
            $"Version=\"1.0.0\", " +
            $"Token=\"{_apiKey}\"");

        var playbackInfoBody = new
        {
            UserId = _userId,
            AutoOpenLiveStream = true,
            IsPlayback = true
        };

        playbackInfoRequest.Content = JsonContent.Create(playbackInfoBody);

        using var playbackInfoResponse = await _httpClient.SendAsync(
            playbackInfoRequest,
            cancellationToken);

        playbackInfoResponse.EnsureSuccessStatusCode();

        var playbackInfo =
            await playbackInfoResponse.Content.ReadFromJsonAsync<PlaybackInfoResponse>(
                cancellationToken);

        if (playbackInfo?.MediaSources == null ||
            playbackInfo.MediaSources.Count == 0)
        {
            throw new PlaybackProviderException("Jellyfin returned no media sources.");
        }

        var mediaSourceId = session.PlayState?.MediaSourceId;

        var mediaSource = playbackInfo.MediaSources
                              .FirstOrDefault(x =>
                                  x.Id != null &&
                                  x.Id.Equals(mediaSourceId, StringComparison.OrdinalIgnoreCase))
                          ?? playbackInfo.MediaSources.FirstOrDefault();

        if (mediaSource?.Path == null)
        {
            throw new PlaybackProviderException("Could not determine the media file path.");
        }

        var mediaPath = mediaSource.Path;

        // Jellyfin stores PositionTicks as 100-nanosecond units.
        // Convert to seconds for ffmpeg.
        var positionTicks = session.PlayState!.PositionTicks!.Value;

        var positionSeconds = positionTicks / 10_000_000.0;

        var subtitles = GetSelectedSubtitles(mediaSource, session.PlayState?.SubtitleStreamIndex);

        return new CurrentPlayback
        {
            MediaPath = mediaPath,
            PositionSeconds = positionSeconds,
            IndexNumber = nowPlayingItem.IndexNumber,
            ParentIndexNumber = nowPlayingItem.ParentIndexNumber,
            SeriesName = nowPlayingItem.SeriesName,
            IsMovie = IsMovie(session),
            Name = nowPlayingItem.Name,
            SubtitlesCodec = subtitles.codec,
            SubtitlesIndex = subtitles.subtitleIndex,
            ExternalSubtitlePath = subtitles.subtitlePath
        };
    }

    private (string? codec, int? subtitleIndex, string? subtitlePath) GetSelectedSubtitles(JellyfinMediaSource? mediaSource, int? subtitleStreamIndex)
    {
        var subtitleStream = mediaSource?.MediaStreams?
                .Where(x => string.Equals(
                    x.Type,
                    "Subtitle",
                    StringComparison.OrdinalIgnoreCase)
                && x.Index == subtitleStreamIndex)
                .Select((stream, index) => new
                {
                    stream,
                    index
                })
                .FirstOrDefault();

        string? path = null;

        if (subtitleStream?.stream.IsExternal != null && subtitleStream?.stream.IsExternal.Value == true)
        {
            path = subtitleStream?.stream.Path;
        }

        return (subtitleStream?.stream.Codec, subtitleStream?.index, path);
    }

    private bool IsMovie(JellyfinSession session)
    {
        return string.Equals( session.NowPlayingItem?.Type, "Movie", StringComparison.OrdinalIgnoreCase);
    }
    
        
    private sealed class JellyfinSession
    {
        public string? Id { get; set; }

        public string? UserId { get; set; }

        public JellyfinPlayState? PlayState { get; set; }

        public JellyfinNowPlayingItem? NowPlayingItem { get; set; }
    }

    private sealed class JellyfinPlayState
    {
        public long? PositionTicks { get; set; }

        public bool IsPaused { get; set; }

        public bool IsMuted { get; set; }

        public string? MediaSourceId { get; set; }

        public string? PlayMethod { get; set; }
        public int? SubtitleStreamIndex { get; set; }
    }

    private sealed class JellyfinNowPlayingItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }
        
        /// <summary>
        /// Movie or Episode
        /// </summary>
        public string? Type { get; set; }
        
        public string? SeriesName { get; set; }
        /// <summary>
        /// Season number
        /// </summary>
        public int? ParentIndexNumber { get; set; }
        /// <summary>
        /// Episode number
        /// </summary>
        public int? IndexNumber { get; set; }

        public List<JellyfinMediaSource>? MediaSources { get; set; }
    }

    private sealed class PlaybackInfoResponse
    {
        public List<JellyfinMediaSource> MediaSources { get; set; } = [];
        public string? PlaySessionId { get; set; }
    }

    private sealed class JellyfinMediaSource
    {
        public string? Id { get; set; }

        public string? Path { get; set; }

        public string? Container { get; set; }

        public List<JellyfinMediaStream>? MediaStreams { get; set; }
    }
    
    private sealed class JellyfinMediaStream
    {
        public string? Codec { get; set; }
        public string? Type { get; set; }
        public int? Index { get; set; }

        public string? Language { get; set; }
        public string? DisplayTitle { get; set; }

        public bool? IsDefault { get; set; }
        public bool? IsForced { get; set; }
        
        public bool? IsExternal { get; set; } 
        public string? Path { get; set; }
    }

}