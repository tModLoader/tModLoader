using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.DownloadManager;

public interface IDownloadProgress
{
	public void DownloadStarted(string displayName);
	public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded);
}

internal class UIWorkshopDownload : UIProgress, IDownloadProgress
{
	internal struct ProgressData
	{
		public string displayName;
		public float progress;
		public long bytesReceived;
		public long totalBytesNeeded;
		public bool reset;
	}
	private ProgressData progressData;
	private bool needToUpdateProgressData = false;

	private Stopwatch downloadTimer;

	public UIWorkshopDownload()
	{
		downloadTimer = new Stopwatch();
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
		// We can't cancel in-progress workshop downloads without getting steam in to a deadlock state - Solxan
		// Steam keeps a cache once a download starts, and doesn't clean up cache until game close, which gets very confusing.
		_cancelButton.Remove();
	}

	public override void Update(GameTime gameTime)
	{
		if (needToUpdateProgressData) { // Lock only when needed
			ProgressData localProgressData;
			lock (this) {
				localProgressData = progressData; // Make local to release the lock
				progressData.reset = false; // Reset reset status

				needToUpdateProgressData = false;
			}

			// Update reset
			if (localProgressData.reset) {
				_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", localProgressData.displayName);
				downloadTimer.Restart();
			}
			// Update progress
			_progressBar.UpdateProgress(localProgressData.progress);
			double elapsedSeconds = downloadTimer.Elapsed.TotalSeconds;
			double speed = elapsedSeconds > 0.0 ? localProgressData.bytesReceived / elapsedSeconds : 0.0;
			SubProgressText = $"{UIMemoryBar.SizeSuffix(localProgressData.bytesReceived, 2)} / {UIMemoryBar.SizeSuffix(localProgressData.totalBytesNeeded, 2)} ({UIMemoryBar.SizeSuffix((long)speed, 2)}/s)";
		}
		base.Update(gameTime);
	}

	/**
	 * <remarks>This will be called from a thread!</remarks>
	 */
	public void DownloadStarted(string displayName)
	{
		lock (this) {
			progressData.displayName = displayName;
			progressData.progress = 0;
			progressData.bytesReceived = 0;
			progressData.totalBytesNeeded = 0;
			progressData.reset = true;

			needToUpdateProgressData = true;
		};
	}

	/**
	 * <remarks>This will be called from a thread!</remarks>
	 */
	public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded)
	{
		lock (this) {
			// Intentional leaving reset and name as previous data (to handle multiple events before UI update
			progressData.progress = progress;
			progressData.bytesReceived = bytesReceived;
			progressData.totalBytesNeeded = totalBytesNeeded;

			needToUpdateProgressData = true;
		};
	}
}