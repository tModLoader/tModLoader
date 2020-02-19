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
			bool modEnabled = ModLoader.GetMod(ModBrowserItem.ModName) != null;
			bool newMod = !ModBrowserItem.HasUpdate;

			if (modEnabled)
				Interface.modBrowser.anEnabledModUpdated = true;
			else if (newMod)
				Interface.modBrowser.aNewModDownloaded = true;
			else
				Interface.modBrowser.aDisabledModUpdated = true;

			if (ModLoader.autoReloadAndEnableModsLeavingModBrowser && newMod)
				ModLoader.EnableMod(ModBrowserItem.ModName);
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
