using System.Diagnostics;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class UIWorkshopDownload : UIProgress, IHaveBackButtonCommand
	{
		public UIState PreviousUIState { get; set; }
		private Stopwatch downloadTimer;

		public UIWorkshopDownload(UIState stateToGoBackTo) {
			//Initialize();
			downloadTimer = new Stopwatch();

			PreviousUIState = stateToGoBackTo;
			Main.menuMode = 888;
		}

		public void PrepUIForDownload(string displayName) {
			_progressBar.UpdateProgress(0f);
			_progressBar.DisplayText = Language.GetTextValue("tModLoader.MBDownloadingMod", displayName);
			downloadTimer.Restart();
			Main.MenuUI.RefreshState();
		}

		public void UpdateDownloadProgress(float progress, long bytesReceived, long totalBytesNeeded) {
			_progressBar.UpdateProgress(progress);

			double elapsedSeconds = downloadTimer.Elapsed.TotalSeconds;
			double speed = elapsedSeconds > 0.0 ? bytesReceived / elapsedSeconds : 0.0;

			SubProgressText = $"{UIMemoryBar.SizeSuffix(bytesReceived, 2)} / {UIMemoryBar.SizeSuffix(totalBytesNeeded, 2)} ({UIMemoryBar.SizeSuffix((long)speed, 2)}/s)";
		}

		public void Leave() {
			(this as IHaveBackButtonCommand).HandleBackButtonUsage();
			Interface.modBrowser.UpdateNeeded = true;
		}
	}
}