using System.Diagnostics;

namespace Shotter.Services;

public class FfmpegService : IFfmpegService
{
    public async Task<(int exitCode, string stderr)> TakeScreenshot(
        CancellationToken cancellationToken,
        double positionSeconds,
        string mediaPath,
        string outputPath,
        bool includeSubtitles)
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

            // When subtitles are included, we call -i before -ss so that the subtitles are loaded correctly at the given timestamp.
            // If subtitles are not included, -ss first before adding -i, which uses less resources and is faster.
            if (includeSubtitles)
            {
                startInfo.ArgumentList.Add("-i");
                startInfo.ArgumentList.Add(mediaPath);
            }

            startInfo.ArgumentList.Add("-ss");
            startInfo.ArgumentList.Add(
                positionSeconds.ToString(
                    System.Globalization.CultureInfo.InvariantCulture));
            
            if (includeSubtitles)
            {
                // TODO: only ASS subtitles seem to work. at least SRT and PGS did not.
                startInfo.ArgumentList.Add("-vf");
                startInfo.ArgumentList.Add(
                        $"subtitles=filename='{mediaPath}':stream_index=0");
            }
            else
            {
                startInfo.ArgumentList.Add("-i");
                startInfo.ArgumentList.Add(mediaPath);
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
            
            return (process.ExitCode, stderr);
        }
        catch
        {
            process?.Dispose();
            throw;
        }
    }
}