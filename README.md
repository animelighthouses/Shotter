# Shotter

A HTTP server for screenshotting current session for Jellyfin and Plex.

## Installation

Build the docker image and load it in. The docker container should run on the same server as your media server, as it needs access to the same file paths.

```yml
services:
  shotter:
    container_name: shotter
    image: shotter:latest
    ports:
      - "8080:8080"
    user: "1000:1000"
    environment:
      MediaServerUrl: # Your Media server url
      MediaServerApiKey: # Your media server API key
      MediaServerUserId: # Your user id for Jellyfin or username for plex
      MediaServerType: # Jellyfin or Plex, removing this defaults to Jellyfin 
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

