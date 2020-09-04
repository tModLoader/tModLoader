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
			bool modEnabled = ModLoader.TryGetMod(ModBrowserItem.ModName, out _);
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
			if (ModLoader.TryGetMod(ModBrowserItem.ModName, out var modInstance)) {
				Logging.tML.Info(Language.GetTextValue("tModLoader.MBReleaseFileHandle", $"{modInstance.Name}: {modInstance.DisplayName}"));
				modInstance?.Close(); // if the mod is currently loaded, the file-handle needs to be released
			}
		}
	}
}
