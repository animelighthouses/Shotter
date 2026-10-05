using System.Diagnostics;
using System.Globalization;

namespace Shotter.Services;

public class FfmpegService : IFfmpegService
{
    private const int PreSeekInSeconds = 5;
    
    public async Task TakeScreenshot(
        CancellationToken cancellationToken,
        double positionSeconds,
        string mediaPath,
        string outputPath,
        bool includeSubtitles,
        int? subtitlesIndex,
        string? subtitlesCodec)
    {
        Process? process = null;
        try
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
                WithSubs(startInfo, positionSeconds, mediaPath, subtitlesIndex, subtitlesCodec);
            }
            else
            {
                WithoutSubs(startInfo, positionSeconds, mediaPath);
            }

            startInfo.ArgumentList.Add("-frames:v");
            startInfo.ArgumentList.Add("1");

            startInfo.ArgumentList.Add("-q:v");
            startInfo.ArgumentList.Add("2");

            startInfo.ArgumentList.Add("-y");
            startInfo.ArgumentList.Add(outputPath);

            process = new Process
            {
                StartInfo = startInfo
            };

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
        catch
        {
            process?.Dispose();
            throw;
        }
    }

    private void WithSubs(
        ProcessStartInfo startInfo,
        double positionSeconds,
        string mediaPath,
        int? subtitlesIndex,
        string? subtitlesCodec)
    {
        // TODO: external subtitles.
        switch (subtitlesCodec?.ToLowerInvariant())
        {
            case "ssa":
            case "ass":
            case "subrip": // SRT
                HandleAsslibSubs(startInfo, positionSeconds, mediaPath, subtitlesIndex);
                break;
            case "pgssub":
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
        int? subtitlesIndex)
    {
        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(positionSeconds.ToString(CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-copyts");

        startInfo.ArgumentList.Add("-vf");
        startInfo.ArgumentList.Add(
            $"subtitles=filename='{mediaPath}':si={subtitlesIndex}");
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
}