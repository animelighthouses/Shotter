using System.Diagnostics;
using System.Globalization;

namespace Shotter.Services;

public class FfmpegService : IFfmpegService
{
    public async Task TakeScreenshot(
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

            if (includeSubtitles)
            {
                WithSubs(startInfo, positionSeconds, mediaPath);
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

    private void WithSubs(ProcessStartInfo startInfo, double positionSeconds, string mediaPath)
    {
        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(positionSeconds.ToString(CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-copyts");

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(mediaPath);

        startInfo.ArgumentList.Add("-vf");
        startInfo.ArgumentList.Add(
            $"subtitles=filename='{mediaPath}':si=0");
    }

    private void WithoutSubs(ProcessStartInfo startInfo, double positionSeconds, string mediaPath)
    {
        startInfo.ArgumentList.Add("-ss");
        startInfo.ArgumentList.Add(
            positionSeconds.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(mediaPath);
    }
}