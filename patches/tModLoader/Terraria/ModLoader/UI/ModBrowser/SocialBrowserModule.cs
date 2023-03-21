using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using System.Threading.Tasks;
using System.Threading;

namespace Terraria.ModLoader.UI.ModBrowser;

public interface SocialBrowserModule
{
	/////// Management of Browser Items ///////////////////////////////////////////

	public List<ModDownloadItem> Items { get; set; }

	// Used for caching in Mod Browser Queries
	internal IReadOnlyList<LocalMod> InstalledItems { get; set; }

	public ModDownloadItem FindDownloadItem(string modName)
		=> Items.FirstOrDefault(x => x.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));

	public QueryConfirmation QueryBrowser(QueryParameters queryParams);
	

	/////// Informed of Local Install ///////////////////////////////////////////

	//TODO: This would need to be public for a mod to add a backend
	internal List<LocalMod> GetInstalledItems();

	internal LocalMod IsItemInstalled(string modSlug)
	{
		if (InstalledItems != null)
			return InstalledItems.FirstOrDefault(m => m.Name == modSlug);

		return GetInstalledItems().FirstOrDefault(m => m.Name == modSlug);
	}

	internal bool DoesItemNeedUpdate(string modId, LocalMod installed, Version webVersion);

	public bool DoesAppNeedRestartToReinstallItem(string modId);

	public bool GetModIdFromLocalFiles(TmodFile modFile, out string modId);

	/////// Management of Downloads ///////////////////////////////////////////

	/// <summary>
	/// Downloads all UIModDownloadItems provided.
	/// </summary>
	internal Task SetupDownload(List<ModDownloadItem> items, int previousMenuId)
	{
		//Set UIWorkshopDownload
		UIWorkshopDownload uiProgress = null;

		// Can't update enabled items due to in-use file access constraints
		var needFreeInUseMods = items.Any(item => item.Installed != null && item.Installed.Enabled);
		if (needFreeInUseMods)
			ModLoader.Unload();

		if (!Main.dedServ) {
			uiProgress = new UIWorkshopDownload(previousMenuId);
			Main.MenuUI.SetState(uiProgress);
		}

		return Task.Run(() => InnerDownload(uiProgress, items, needFreeInUseMods));
	}

	private void InnerDownload(UIWorkshopDownload uiProgress, List<ModDownloadItem> items, bool reloadWhenDone)
	{
		foreach (var item in items) {
			DownloadItem(item, uiProgress);

			// Due to issues with Steam moving files from downloading folder to installed folder,
			// there can be some latency in detecting it's installed. - Solxan
			Thread.Sleep(1000);

			// Add installed info to the downloaded item
			var localMod = GetInstalledItems().FirstOrDefault(m => m.Name == item.ModName);
			FindDownloadItem(item.ModName).Installed = localMod;
		}

		Interface.modBrowser.PopulateModBrowser(uiOnly: true);
		Interface.modBrowser.UpdateNeeded = true;

		uiProgress?.Leave(refreshBrowser: true);

		if (reloadWhenDone)
			ModLoader.Reload();
	}

	internal void DownloadItem(ModDownloadItem item, UIWorkshopDownload uiProgress);
}


public struct QueryConfirmation
{
	public bool success;
	public int pageSize;
	public int totalItems;
}

public struct QueryParameters
{
	public List<string> searchTags;
	public List<string> searchModIds;
	public List<string> searchModSlugs;
	public string searchTextField;

	public UIBrowserFilterToggle<ModBrowserSortMode> sortingParamater;
	public UIBrowserFilterToggle<UpdateFilter> updateStatusFilter;
	public UIBrowserFilterToggle<SearchFilter> searchFilterOption;
	public UIBrowserFilterToggle<ModSideFilter> modSideFilter;

	public QueryType queryType;
}

public enum QueryType
{
	SearchAll,
	SearchDirect
}
