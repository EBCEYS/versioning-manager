using System.Diagnostics;

namespace ServiceUploader.Extensions;

public static class StreamExtensions
{
    public static async Task DownloadFileAsync(this Stream contentStream, string destination,
        Action<long, double> downloadProgress, Action<long, double> finishProcess, TimeSpan period,
        CancellationToken cancellationToken = default)
    {
        await using FileStream fileStream = new(
            destination,
            FileMode.Create,
            FileAccess.Write);

        var buffer = new byte[8192];
        Stopwatch stopwatch = new();
        long totalRead = 0;
        long lastBytes = 0;


        Timer progressTimer = new(UpdateProgress, null, 0, Convert.ToInt32(period.TotalMilliseconds));

        stopwatch.Start();
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalRead += bytesRead;
        }

        stopwatch.Stop();
        await progressTimer.DisposeAsync();

        finishProcess(totalRead, stopwatch.Elapsed.TotalSeconds);
        return;

        void UpdateProgress(object? state)
        {
            if (totalRead <= 0) return;

            var currentSpeed = (totalRead - lastBytes) / (period.TotalSeconds * 1024.0 * 1024.0);
            lastBytes = totalRead;


            downloadProgress(totalRead, currentSpeed);
        }
    }
}