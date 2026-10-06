using System.Buffers;
using System.Net;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core.Utilities {
    public static class DownloadHelpers {

        public static async Task<MemoryStream> DownloadWithProgress(HttpClient client, string url, ITaskProgress progress, CancellationToken cancellationToken = default) {
            var responseTask = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			progress.ReportStatus("Request sent...");

			using var response = await responseTask;
			response.EnsureSuccessStatusCode();

			long size = response.Content.Headers.ContentLength 
				?? throw new WebException("Expected ContentLength header", WebExceptionStatus.ReceiveFailure);

			progress.ReportStatus("Downloading data...");

			var stream = new MemoryStream((int)size);

            using var body = await response.Content.ReadAsStreamAsync();

			await CopyWithProgress(body, stream, (int)size, progress);

            return stream;
        }

        public static async Task CopyWithProgress(Stream from, Stream to, int expectedSize, ITaskProgress progress, CancellationToken cancellationToken = default) {
            progress.SetMaxProgress(expectedSize / 1024);

            byte[]? buffer = null;
            bool directBuffer = false;
            int directStart = 0;

            if (to is MemoryStream ms) {
                try {
                    directStart = (int)ms.Position;
                    ms.SetLength((long)(expectedSize + directStart));
                    buffer = ms.GetBuffer();
                    directBuffer = true;
                }
                catch (UnauthorizedAccessException) {}
            }

            if (buffer is null) {
                buffer = ArrayPool<byte>.Shared.Rent(81920);
                directBuffer = false;
            }
            
            int copied = 0;

            while (!cancellationToken.IsCancellationRequested) {

                int readStart = directBuffer? copied + directStart : 0;

                int read = await from.ReadAsync(buffer, readStart, buffer.Length - readStart, cancellationToken);
                if (read == 0) {
                    break;
                }

                copied += read;
                progress.SetCurrentProgress(copied / 1024);

                if (!directBuffer) {
                    await to.WriteAsync(buffer, 0, read, cancellationToken);
                }
            }

            if (!directBuffer) {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}