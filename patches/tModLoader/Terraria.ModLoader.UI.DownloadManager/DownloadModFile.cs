using System.Collections;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.UI.ModBrowser;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class DownloadModFile : DownloadFile
	{
		public UIModDownloadItem ModBrowserItem;

		public DownloadModFile(string url, string filePath, string displayText) : base(url, filePath, displayText) {
			OnComplete += ProcessDownloadedMod;
		}

		private void ProcessDownloadedMod() {
			var mod = ModLoader.GetMod(ModBrowserItem.ModName);
			if (mod != null) {
				Interface.modBrowser.anEnabledModDownloaded = true;
			}

			if (!ModBrowserItem.HasUpdate) Interface.modBrowser.aNewModDownloaded = true;
			else Interface.modBrowser.aModUpdated = true;

			if (ModLoader.autoReloadAndEnableModsLeavingModBrowser) ModLoader.EnableMod(ModBrowserItem.ModName);
			Interface.modBrowser.RemoveItem(ModBrowserItem);
			Interface.modBrowser.UpdateNeeded = true;
		}

		internal override void PreCopy() {
			var modInstance = ModLoader.GetMod(ModBrowserItem.ModName);
			if (modInstance != null) {
				Logging.tML.Info(Language.GetTextValue("tModLoader.MBReleaseFileHandle", $"{modInstance.Name}: {modInstance.DisplayName}"));
				modInstance?.Close(); // if the mod is currently loaded, the file-handle needs to be released
			}
		}
	}
}
