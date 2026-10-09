# Shotter

A HTTP server for screenshotting current session for Jellyfin and Plex.

## Get started

You can use Shotter with the docker compose file below.

```yml
services:
  shotter:
    container_name: shotter
    image: ghcr.io/voidnyan/shotter:latest
    ports:
      - "8080:8080"
    user: "1000:1000"
    environment:
      MediaServerUrl: # Your Media server url
      MediaServerApiKey: # Your media server API key
      MediaServerUserId: # Optional, user id for Jellyfin or username for plex. Used to specify the user whose activity will be screenshotted
      MediaServerType: # Jellyfin or Plex, removing this defaults to Jellyfin
      NotificationProvider: # Optional, supported providers: Ntfy
      Ntfy__Url: # Ntfy service url
      Ntfy__Topic: # The ntfy topic the notification will be sent
      Ntfy__AccessToken: # Optional, needed if the topic is protected
    volumes:
      - ./screenshots:/screenshots
      - /path/to/media:/media:ro
      - /path/to/media2:/media2:ro
    restart: unless-stopped
```

The media volumes should match what you have for your media server.

## Usage

Calling `http://<yourshotteraddress>/api/screenshot` will queue taking a screenshot. 
It is recommended to pause the stream before the call.

Calling `/api/screenshot?includeSubtitles=true` queues a screenshot job that will include subtitles.
This process is heavier on the system resources.

You can also call `/api/is-processing`, which will return `true`, if the server is currently processing a screenshot job
and `false` if not.

## Notifications

You can configure Shotter to send you a notification if it fails to process a screenshot job.
Currently only Ntfy is supported as a notification provider.

You can call `/api/test-notification` to send a test notification to verify that they work.
