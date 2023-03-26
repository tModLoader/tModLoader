using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.UI.ModBrowser;

public interface SocialBrowserModule
{
	/////// Management of Browser Items ///////////////////////////////////////////

	public List<ModDownloadItem> Items { get; set; }

	// Used for caching in Mod Browser Queries
	//TODO: handling installed ModDowmloadItem for query
	internal IReadOnlyList<LocalMod> InstalledItems { get; set; }

	public ModDownloadItem FindDownloadItem(string modName)
		=> Items.FirstOrDefault(x => x.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));

#pragma warning disable CS8424 // I know [EnumeratorCancellation] has no effect, but it's placed here to remember to add it to async implementations
	public IAsyncEnumerable<ModDownloadItem> QueryBrowser(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token = default);
#pragma warning restore CS8424

	public ModDownloadItem[] DirectQueryItems(QueryParameters queryParams);

	/////// Display of Browser Items ///////////////////////////////////////////

	public string GetModWebPage(string modId);

	/////// Management of Local Install ///////////////////////////////////////////

	//TODO: This would need to be public for a mod to add a backend
	internal IReadOnlyList<LocalMod> GetInstalledItems();

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

	/////// Management of Dependencies ///////////////////////////////////////////

	public ModDownloadItem[] GetDependencies(HashSet<string> modIds);

	public void GetDependenciesRecursive(HashSet<string> modIds, ref HashSet<ModDownloadItem> set)
	{
		var deps = GetDependencies(modIds);
		set.UnionWith(deps);

		HashSet<string> depIds = deps.Select(d => d.PublishId).Except(set.Select(d => d.PublishId)).ToHashSet();

		//TODO: What if the same mod is a dependency twice, but different versions?
		GetDependenciesRecursive(depIds, ref set);
	}

	public static string GetBrowserVersionNumber(Version tmlVersion)
	{
		if (tmlVersion < new Version(0, 12)) // Versions 0 to 0.11.8.9
			return "1.3";

		if (tmlVersion < new Version(2022, 10)) // Versions 0.12 to 2022.9
			return "1.4";

		return "1.4.4";
	} 
}


public struct QueryConfirmation
{
	public bool success;
	public int pageSize;
	public int totalItems;
}

public struct QueryParameters
{
	public string[] searchTags;
	public string[] searchModIds;
	public string[] searchModSlugs;
	public string searchGeneric;
	public string searchAuthor;

	public ModBrowserSortMode sortingParamater;
	public UpdateFilter updateStatusFilter;
	public ModSideFilter modSideFilter;

	public QueryType queryType;
}

public enum QueryType
{
	SearchAll,
	SearchDirect
}
