using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.DownloadManager;

public interface IDownloadProgress
{
	public void DownloadStarted(string displayName);
	public void DownloadCompleted();
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

	public int PreviousMenuMode { get; set; } = -1;

	public UIWorkshopDownload(int previousMenuMode)
	{
		downloadTimer = new Stopwatch();
		PreviousMenuMode = previousMenuMode;
		Main.menuMode = 888;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
		_cancelButton.Remove(); // Why cannot cancel???
	}

	public override void Update(GameTime gameTime)
	{
		if (needToUpdateProgressData) {
			lock (this) {
				// Update reset
				if (progressData.reset) {
					_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", progressData.displayName);
					downloadTimer.Restart();
					Main.MenuUI.RefreshState();
				}
				// Update progress
				_progressBar.UpdateProgress(progressData.progress);
				double elapsedSeconds = downloadTimer.Elapsed.TotalSeconds;
				double speed = elapsedSeconds > 0.0 ? progressData.bytesReceived / elapsedSeconds : 0.0;
				SubProgressText = $"{UIMemoryBar.SizeSuffix(progressData.bytesReceived, 2)} / {UIMemoryBar.SizeSuffix(progressData.totalBytesNeeded, 2)} ({UIMemoryBar.SizeSuffix((long)speed, 2)}/s)";

				needToUpdateProgressData = false;
			}
		}
		base.Update(gameTime);
	}

	/**
	 * <remarks>This will be called from a thread!</remarks>
	 */
	public void DownloadStarted(string displayName)
	{
		lock (this) {
			progressData = new ProgressData {
				displayName = displayName,
				progress = 0,
				bytesReceived = 0,
				totalBytesNeeded = 0,
				reset = true
			};
			needToUpdateProgressData = true;
		};
	}

	/**
	 * <remarks>This will be called from a thread!</remarks>
	 */
	public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded)
	{
		lock (this) {
			progressData = new ProgressData {
				displayName = "",
				progress = progress,
				bytesReceived = bytesReceived,
				totalBytesNeeded = totalBytesNeeded,
				reset = false
			};
			needToUpdateProgressData = true;
		};
	}
	/**
	* <remarks>This will be called from a thread!</remarks>
	*/
	public void DownloadCompleted()
	{
		// @TODO: Is this ok in a thread?
		ReturnToPreviousMenu();
	}

	public void ReturnToPreviousMenu()
	{
		if (PreviousMenuMode == -1) {
			Main.menuMode = 0;
			return;
		}

		if (PreviousMenuMode != -1) {
			Main.menuMode = PreviousMenuMode;
		}

		SoundEngine.PlaySound(11);
	}
}