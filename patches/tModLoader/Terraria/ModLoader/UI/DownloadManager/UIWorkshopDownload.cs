using System.Diagnostics;
using Terraria.Audio;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.DownloadManager;

internal class UIWorkshopDownload : UIProgress
{
	private Stopwatch downloadTimer;

	public int PreviousMenuMode { get; set; } = -1;

	public UIWorkshopDownload(int previousMenuMode)
	{
		downloadTimer = new Stopwatch();
		PreviousMenuMode = previousMenuMode;
		Main.menuMode = 888;
	}

	public void PrepUIForDownload(string displayName)
	{
		_progressBar.UpdateProgress(0f);
		_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", displayName);
		downloadTimer.Restart();
		Main.MenuUI.RefreshState();

		_cancelButton.Remove();
	}

	public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded)
	{
		_progressBar.UpdateProgress(progress);

		double elapsedSeconds = downloadTimer.Elapsed.TotalSeconds;
		double speed = elapsedSeconds > 0.0 ? bytesReceived / elapsedSeconds : 0.0;

		SubProgressText = $"{UIMemoryBar.SizeSuffix(bytesReceived, 2)} / {UIMemoryBar.SizeSuffix(totalBytesNeeded, 2)} ({UIMemoryBar.SizeSuffix((long)speed, 2)}/s)";
	}

	public void Leave(bool refreshBrowser)
	{
		// Due to issues with Steam moving files from downloading folder to installed folder,
		// there can be some latency in detecting it's installed. - Solxan
		System.Threading.Thread.Sleep(50);

		// Re-populate the mod Browser so that the "Installed" information refreshes.
		if (refreshBrowser) {
			Interface.modBrowser.PopulateModBrowser();
			Interface.modBrowser.UpdateNeeded = true;
		}

		// Exit
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