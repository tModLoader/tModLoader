using System.Net;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core.Utilities {
	public static class DownloadHelpers {

		public static async Task<MemoryStream> DownloadWithProgress(HttpClient client, string url, ITaskProgress progress, CancellationToken cancellationToken = default) {
			using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			response.EnsureSuccessStatusCode();

			var size = (int?)response.Content.Headers.ContentLength
				?? throw new WebException("Expected ContentLength header", WebExceptionStatus.ReceiveFailure);

			progress.ReportStatus($"Downloading {FormatSize(size)}...");
			progress.SetMaxProgress(size / 1024);

			using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
			var buffer = new byte[size];

			for (int copied = 0; copied < size; ) {
				int read = await body.ReadAsync(buffer.AsMemory(copied), cancellationToken);
				if (read == 0)
					throw new EndOfStreamException();

				copied += read;
				progress.SetCurrentProgress(copied / 1024);
			}

			progress.SetMaxProgress(0); // no need to leave the download progress at the top
			return new MemoryStream(buffer);
		}

		private static string FormatSize(long bytes) {
			string[] units = ["B", "KB", "MB", "GB"];
			double size = bytes;
			int mag = 0;
			for (; size >= 1024 && mag < units.Length - 1; mag++)
				size /= 1024;

			return $"{size:0.#} {units[mag]}";
		}
	}
}