using Terraria.ModLoader.UI.ModBrowser;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class DownloadModFile : DownloadFile
	{
		public UIModDownloadItem ModBrowserItem;

		public DownloadModFile(string url, string filePath, string displayText) : base(url, filePath, displayText) {
		}
	}
}
