using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using Shotter.Core.Configuration;
using Shotter.Core.Interfaces;

namespace Shotter.Services.Screenshot;

public class ScreenshotProcessor(IOptions<FfmpegOptions> options) : IScreenshotProcessor
{
    private const int PreSeekInSeconds = 5;
    private readonly string _fileExtension = options.Value.ScreenshotFileExtension;
    private readonly string? _quality = options.Value.ScreenshotQuality;
    
    public async Task TakeScreenshot(
        CancellationToken cancellationToken,
        double positionSeconds,
        string mediaPath,
        string outputPath,
        bool includeSubtitles,
        int? subtitlesIndex,
        string? subtitlesCodec,
        string? externalSubtitlePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (includeSubtitles)
        {
            WithSubs(startInfo, positionSeconds, mediaPath, subtitlesIndex, subtitlesCodec, externalSubtitlePath);
        }
        else
        {
            WithoutSubs(startInfo, positionSeconds, mediaPath);
        }

        startInfo.ArgumentList.Add("-frames:v");
        startInfo.ArgumentList.Add("1");

        SetQuality(startInfo);

        startInfo.ArgumentList.Add("-y");
        startInfo.ArgumentList.Add(outputPath + "." + _fileExtension);

        using var process = new Process();
        process.StartInfo = startInfo;

        process.Start();

        var stderrTask = process.StandardError.ReadToEndAsync(
            cancellationToken);

        var stdoutTask = process.StandardOutput.ReadToEndAsync(
            cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stderr = await stderrTask;
        _ = await stdoutTask;
        
        if (process.ExitCode != 0)
        {
            throw new Exception(stderr);
        }
    }

    private static void WithSubs(
        ProcessStartInfo startInfo,
        double positionSeconds,
        string mediaPath,
        int? subtitlesIndex,
        string? subtitlesCodec,
        string? externalSubtitlePath)
    {
        switch (subtitlesCodec?.ToLowerInvariant())
        {
            case "ssa":
            case "ass":
            case "subrip": // SRT
                HandleAsslibSubs(startInfo, positionSeconds, mediaPath, subtitlesIndex, externalSubtitlePath);
                break;
            case "pgssub":
            case "pgs":
            case "vobsub":
                HandlePgsSubs(startInfo, positionSeconds, mediaPath, subtitlesIndex);
                break;
            default:
                WithoutSubs(startInfo, positionSeconds, mediaPath);
                break;
        }
    }
    
    private static void HandlePgsSubs(ProcessStartInfo startInfo, double positionSeconds, string mediaPath, int? subtitlesIndex)
    {
        // Seeking directy to the target timestamp resulted in the subtitles not being rendered.
        // Seek a few seconds before the target first.
        var seekPosition = Math.Max(0, positionSeconds - PreSeekInSeconds);

        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(
            seekPosition.ToString(CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-copyts");

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(mediaPath);

        startInfo.ArgumentList.Add("-filter_complex");
        startInfo.ArgumentList.Add(
            $"[0:v:0][0:s:{subtitlesIndex}]overlay[overlaid];" +
            $"[overlaid]select='gte(t\\,{positionSeconds.ToString(CultureInfo.InvariantCulture)})'[out]");

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("[out]");
    }

    private static void HandleAsslibSubs(
        ProcessStartInfo startInfo,
        double positionSeconds,
        string mediaPath,
        int? subtitlesIndex,
        string? externalSubtitlePath)
    {
        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(positionSeconds.ToString(CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-copyts");
        
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(mediaPath);
        
        startInfo.ArgumentList.Add("-vf");
        startInfo.ArgumentList.Add(string.IsNullOrEmpty(externalSubtitlePath)
            ? $"subtitles=filename='{mediaPath}':si={subtitlesIndex}"
            : $"subtitles=filename='{externalSubtitlePath}'");
    }

    private static void WithoutSubs(ProcessStartInfo startInfo, double positionSeconds, string mediaPath)
    {
        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(
            positionSeconds.ToString(
                CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(mediaPath);
    }

    private void SetQuality(ProcessStartInfo startInfo)
    {
        switch (_fileExtension.ToLowerInvariant())
        {
            case "jpg":
            case "jpeg":
                startInfo.ArgumentList.Add("-q:v");
                startInfo.ArgumentList.Add(!string.IsNullOrEmpty(_quality) ? _quality : "2");
                break;

            case "webp":
                startInfo.ArgumentList.Add("-quality");
                startInfo.ArgumentList.Add(!string.IsNullOrEmpty(_quality) ? _quality : "90");
                break;

            case "png":
                startInfo.ArgumentList.Add("-compression_level");
                startInfo.ArgumentList.Add(!string.IsNullOrEmpty(_quality) ? _quality : "6");
                break;
        }
    }
}