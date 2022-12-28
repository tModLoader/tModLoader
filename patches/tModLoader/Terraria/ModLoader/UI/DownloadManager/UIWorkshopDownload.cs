using System.Diagnostics;
using System.Linq;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class UIWorkshopDownload : UIProgress
	{
		private Stopwatch downloadTimer;

		public int PreviousMenuMode { get; set; } = -1;

		public UIWorkshopDownload(int previousMenuMode) {
			downloadTimer = new Stopwatch();
			PreviousMenuMode = previousMenuMode;
			Main.menuMode = 888;
		}

		public void PrepUIForDownload(string displayName) {
			_progressBar.UpdateProgress(0f);
			_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", displayName);
			downloadTimer.Restart();
			Main.MenuUI.RefreshState();

			_cancelButton.Remove();
		}

		public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded) {
			_progressBar.UpdateProgress(progress);

			double elapsedSeconds = downloadTimer.Elapsed.TotalSeconds;
			double speed = elapsedSeconds > 0.0 ? bytesReceived / elapsedSeconds : 0.0;

			SubProgressText = $"{UIMemoryBar.SizeSuffix(bytesReceived, 2)} / {UIMemoryBar.SizeSuffix(totalBytesNeeded, 2)} ({UIMemoryBar.SizeSuffix((long)speed, 2)}/s)";
		}

		public void Leave(bool refreshBrowser) {
			// Exit
			ReturnToPreviousMenu();
		}

		public void ReturnToPreviousMenu() {
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
}